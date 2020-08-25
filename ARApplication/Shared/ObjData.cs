using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Urho;
using Windows.ApplicationModel;

namespace BodyAR {
    class ObjData {

        public struct ObjectData {
            public string name;
            public List<IntVector3> faces;

            public ObjectData(string name) {
                this.name = name;
                faces = new List<IntVector3>();
            }
        }

        public List<ObjectData> objects;
        public List<Vector3> positions;
        public List<Vector3> normals;
        public List<Vector2> uvs;

        public ObjData() {
            objects = new List<ObjectData>();
            positions = new List<Vector3>();
            normals = new List<Vector3>();
            uvs = new List<Vector2>();
        }

        public float[] GetVertices(string objectName) {
            var faces = objects.Where(o => o.name == objectName).First().faces;
            float[] vertices = new float[faces.Count * 3];
            for(int i = 0; i < faces.Count; ++i) {
                var vertex = positions[faces[i].X];
                vertices[3 * i + 0] = vertex.X;
                vertices[3 * i + 1] = vertex.Y;
                vertices[3 * i + 2] = vertex.Z;
            }
            return vertices;
        }

        public static async Task<ObjData> LoadObjFromFile(string filename) {
            var file = await Package.Current.InstalledLocation.GetFileAsync(filename);
            using(var stream = await file.OpenStreamForReadAsync()) {
                return LoadObjFromStream(stream);
            }
        }

        public static ObjData LoadObjFromStream(Stream stream) {
            var obj = new ObjData();

            using(var reader = new StreamReader(stream)) {
                string line;
                ObjectData currentObject = new ObjectData("");

                while((line = reader.ReadLine()) != null) {
                    if(string.IsNullOrWhiteSpace(line) || line.Length < 2)
                        continue;

                    switch(line[0]) {
                        case '#':
                            continue;
                        case 'o':
                            if(currentObject.faces.Count > 0) {
                                obj.objects.Add(currentObject);
                            }
                            currentObject = new ObjectData(line.Substring(2));
                            break;
                        case 'v':
                            obj.ParseVertexLine(line);
                            break;
                        case 'f':
                            obj.ParseFaceLine(line, ref currentObject);
                            break;
                    }
                }
                if(currentObject.faces.Count > 0) {
                    obj.objects.Add(currentObject);
                }
            }

            return obj;
        }

        private void ParseVertexLine(string line) {
            var parts = line.Split(' ');
            switch(parts[0]) {
                case "v":
                    positions.Add(new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])));
                    break;
                case "vt":
                    uvs.Add(new Vector2(float.Parse(parts[1]), 1.0f - float.Parse(parts[2])));
                    break;
                case "vn":
                    normals.Add(new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])));
                    break;
            }
        }

        private static Regex faceRegex = new Regex(@"f ((\d+)(?:\/(\d+)?(?:\/(\d+))?)?) ((\d+)(?:\/(\d+)?(?:\/(\d+))?)?) ((\d+)(?:\/(\d+)?(?:\/(\d+))?)?)");
        private void ParseFaceLine(string line, ref ObjectData obj) {
            var match = faceRegex.Match(line);
            if(!match.Success) {
                return;
            }

            foreach(var groupStart in new int[]{ 2, 6, 10 }) {
                var faceIndex = new IntVector3();
                if(!int.TryParse(match.Groups[groupStart + 0].Value, out faceIndex.X)) {
                    faceIndex.X = 0; // no position set
                }
                if(!int.TryParse(match.Groups[groupStart + 1].Value, out faceIndex.Y)) {
                    faceIndex.Y = 0; // no texCoord set
                }
                if(!int.TryParse(match.Groups[groupStart + 2].Value, out faceIndex.Z)) {
                    faceIndex.Z = 0; // no normal set
                }
                // ensure zero-indexing
                faceIndex.X -= 1;
                faceIndex.Y -= 1;
                faceIndex.Z -= 1;
                obj.faces.Add(faceIndex);
            }
        }
    }
}
