using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;

namespace BodyAR {
    class Sprite : Component {
        private bool visible;
        private BillboardSet billboards;

        public Sprite() {
            var node = FloatManager.Inst.Node.CreateChild();
            node.Position = new Vector3(0, 0, 0);

            node.AddComponent(this);
            Init();

            visible = true;
        }

        private void Init() {
            var cache = Application.ResourceCache;

            billboards = Node.CreateComponent<BillboardSet>();
            billboards.NumBillboards = 1;
            billboards.Material = cache.GetMaterial("Materials/Sprite.xml").Clone();

            Node.Position = Position;

            var billboard = billboards.GetBillboardSafe(0);
            billboard.Position = Vector3.Zero;
            billboard.Size = new Vector2(0.1f, 0.1f);
            billboard.Enabled = true;
            billboards.Commit();
        }

        public Boolean Visible {
            get {
                return visible;
            }
            set {
                if(value == visible) {
                    return;
                }

                visible = value;
                var billboard = billboards.GetBillboardSafe(0);
                billboard.Enabled = visible;
                billboards.Commit();
            }
        }

        public string TextureID {
            set {
                var texture = Application.ResourceCache.GetTexture2D(value);
                if(texture != null) {
                    billboards.Material.SetTexture(TextureUnit.Diffuse, texture);
                }
            }
        }

        public Vector3 Position {
            get {
                return Node.Position;
            }
            set {
                Node.Position = value;
            }
        }
    }
}
