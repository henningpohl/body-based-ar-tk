using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;

namespace BodyAR {
    class Path : Component {
        private List<Vector3> positions = new List<Vector3>();
        private List<Vector3> tangents = new List<Vector3>();
        private List<Vector3> normals = new List<Vector3>();
        private List<Vector3> binormals = new List<Vector3>();

        private const int RADIAL_SEGMENTS = 5;
        private const float RADIUS = 0.05f;

        private Model model;
        private StaticModel sm;
        private Material material;

        private Geometry geom;
        private VertexBuffer vb;
        private IndexBuffer ib;

        private uint vbSize = 0;
        private uint ibSize = 0;

        private Path() {

        }

        public static Path Create() {
            var node = Application.Current.GetScene().CreateChild();
            var path = new Path();
            node.AddComponent(path);
            path.Init();
            return path;
        }

        private void Init() {
            vb = new VertexBuffer(Application.Context, false);
            ib = new IndexBuffer(Application.Context, false);

            geom = new Geometry();
            geom.SetVertexBuffer(0, vb);
            geom.IndexBuffer = ib;

            model = new Model();
            model.NumGeometries = 1;
            model.SetGeometry(0, 0, geom);
            model.BoundingBox = new BoundingBox(new Vector3(0, 0, 0), new Vector3(0, 0, 0));

            sm = Node.CreateComponent<StaticModel>();
            material = Application.ResourceCache.GetMaterial("Materials/ColoredPath.xml");
            material.SetShaderParameter("MatDiffColor", Color.White);
            ForceModelUpdate();
        }

        public void Update(IEnumerable<Vector3> newPath) {
            positions.Clear();
            positions.AddRange(newPath);

            if(positions.Count < 2) {
                return;
            }

            ComputeTangentsNormalsAndBinormals();
            UpdateGeometry();

            Vector3 bboxmin = positions[0], bboxmax = positions[0];
            foreach(var pos in positions) {
                bboxmin = Vector3.ComponentMin(bboxmin, pos);
                bboxmax = Vector3.ComponentMax(bboxmax, pos);
            }
            model.BoundingBox = new BoundingBox(bboxmin, bboxmax);

            ForceModelUpdate();
        }

        // StaticModel only pulls the updated bounding box when given a _new_ model O_o
        private void ForceModelUpdate() {
            model.AddRef();
            sm.Model = null;
            sm.Model = model;
            model.ReleaseRef();
            sm.SetMaterial(material);
        }

        // https://github.com/mrdoob/three.js/blob/master/src/geometries/TubeGeometry.js
        // https://threejs.org/docs/#api/en/geometries/TubeGeometry
        private void UpdateGeometry() {
            List<float> vertices = new List<float>();
            List<short> indices = new List<short>();

            for(int i = 0; i < positions.Count; ++i) {
                for(int j = 0; j <= RADIAL_SEGMENTS; ++j) {
                    var v = ((double)j / RADIAL_SEGMENTS) * Math.PI * 2.0;
                    var s = Math.Sin(v);
                    var c = -Math.Cos(v);

                    var normal = Vector3.Multiply(normals[i], (float)c) + Vector3.Multiply(binormals[i], (float)s);
                    normal.Normalize();

                    vertices.Add(positions[i].X + RADIUS * normal.X); // p.x
                    vertices.Add(positions[i].Y + RADIUS * normal.Y); // p.y
                    vertices.Add(positions[i].Z + RADIUS * normal.Z); // p.z
                    vertices.Add(normal.X); // n.x
                    vertices.Add(normal.Y); // n.y
                    vertices.Add(normal.Z); // n.z
                }
            }
            for(int i = 1; i < positions.Count; ++i) {
                for(int j = 1; j <= RADIAL_SEGMENTS; ++j) {
                    var a = (RADIAL_SEGMENTS + 1) * (i - 1) + (j - 1);
                    var b = (RADIAL_SEGMENTS + 1) * i + (j - 1);
                    var c = (RADIAL_SEGMENTS + 1) * i + j;
                    var d = (RADIAL_SEGMENTS + 1) * (i - 1) + j;

                    indices.Add((short)a);
                    indices.Add((short)b);
                    indices.Add((short)d);
                    indices.Add((short)b);
                    indices.Add((short)c);
                    indices.Add((short)d);
                }
            }

            if(vertices.Count > vbSize) {
                while(vertices.Count > vbSize) {
                    vbSize += 1024;
                }
                vb.SetSize(vbSize, ElementMask.Position | ElementMask.Normal, true);
            }

            // somehow the discard flag is important, otherwise an E_INVALIDARG exception is thrown in a internal map call           
            unsafe {
                var vertexArray = vertices.ToArray();
                fixed (float* p = &vertexArray[0]) {
                    vb.SetDataRange(p, 0, (uint)vertexArray.Length / 6, true);
                }
            }

            if(indices.Count > ibSize) {
                while(indices.Count > ibSize) {
                    ibSize += 1024;
                }
                ib.SetSize(ibSize, false, true);
            }

            // somehow the discard flag is important, otherwise an E_INVALIDARG exception is thrown in a internal map call           
            unsafe {
                var indexArray = indices.ToArray();
                fixed (short* p = &indexArray[0]) {
                    ib.SetDataRange(p, 0, (uint)indexArray.Length, true);
                }
            }

            geom.SetDrawRange(PrimitiveType.TriangleList, 0, (uint)indices.Count, false);
        }
        
        // https://github.com/mrdoob/three.js/blob/master/src/extras/core/Curve.js
        private void ComputeTangentsNormalsAndBinormals() {
            // compute tangents
            for(int i = 0; i < positions.Count; ++i) {
                var tangent = Vector3.Zero;
                if(i == 0) {
                    tangent = positions[1] - positions[0];
                } else if(i == positions.Count - 1) {
                    tangent = positions[positions.Count - 1] - positions[positions.Count - 2];
                } else {
                    tangent = 0.5f * (positions[i] - positions[i - 1]) + 0.5f * (positions[i + 1] - positions[i]);
                }
                tangent.Normalize();
                tangents.Add(tangent);
            }

            // compute normals and binormals
            var init_normal = new Vector3();
            var tx = Math.Abs(tangents[0].X);
            var ty = Math.Abs(tangents[0].Y);
            var tz = Math.Abs(tangents[0].Z);
            var min = Single.MaxValue;
            if(tx <= min) {
                min = tx;
                init_normal = new Vector3(1, 0, 0);
            }
            if(ty <= min) {
                min = ty;
                init_normal = new Vector3(0, 1, 0);
            }
            if(tz <= min) {
                init_normal = new Vector3(0, 0, 1);
            }
            Vector3 temp = Vector3.Cross(tangents[0], init_normal);
            temp.Normalize();

            normals.Add(Vector3.Cross(tangents[0], temp));
            binormals.Add(Vector3.Cross(tangents[0], normals[0]));

            for(int i = 1; i < positions.Count; ++i) {
                var change = Vector3.Cross(tangents[i - 1], tangents[i]);
                if(change.Length > Single.Epsilon) {
                    change.Normalize();
                    var theta = (float)Math.Acos(Math.Clamp(Vector3.Dot(tangents[i - 1], tangents[i]), -1, 1));
                    normals.Add(Vector3.Transform(normals[i - 1], Matrix4.CreateFromAxisAngle(change, theta)).Xyz);
                } else {
                    normals.Add(normals[i - 1]);
                }
                binormals.Add(Vector3.Cross(tangents[i], normals[i]));
            }
        }

        public Color Color {
            set {
                material.SetShaderParameter("MatDiffColor", value);
            }
        }
    }
}
