using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Gui;

namespace BodyAR {
    class Label : Component {
        private bool visible;
        private Text label;
        private BorderImage background;
        private UIComponent panel;
        private BillboardSet billboards;

        public Label(string text) {
            var node = FloatManager.Inst.Node.CreateChild();
            node.Position = new Vector3(0, 0, 0);

            node.AddComponent(this);
            Init();

            Text = text;
            visible = true;
        }

        private void Init() {
            var cache = Application.ResourceCache;

            label = new Text() {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            label.SetColor(new Color(1.0f, 1.0f, 1.0f));
            label.SetFont(cache.GetFont("Fonts/OpenSans-Bold.ttf"), 20);

            background = new BorderImage() {
                //Texture = cache.GetTexture2D("Textures/UI.png"),
                //ImageRect = new IntRect(48, 0, 64, 16),
                //Border = new IntRect(4, 4, 4, 4),
                Texture = cache.GetTexture2D("Textures/Panel.png"),
                ImageRect = new IntRect(0, 0, 64, 64),
                Border = new IntRect(30, 30, 30, 30),
                BlendMode = BlendMode.Alpha,
            };

            panel = Node.CreateComponent<UIComponent>();
            panel.Material.SetTechnique(0, cache.GetTechnique("Techniques/DiffUnlitAlpha.xml"));
            panel.Root.LayoutMode = LayoutMode.Free;
            panel.Root.HorizontalAlignment = HorizontalAlignment.Center;
            panel.Root.VerticalAlignment = VerticalAlignment.Center;
            panel.Root.AddChild(background);
            panel.Root.AddChild(label);

            billboards = Node.CreateComponent<BillboardSet>();
            billboards.NumBillboards = 1;
            billboards.Material = panel.Material;

            Node.Position = Position;

            var billboard = billboards.GetBillboardSafe(0);
            billboard.Enabled = false;
            billboards.Commit();


            /*
            var node = Node.CreateChild();
            node.Position = new Vector3(0.0f, 2.0f, 0.0f);
            node.Scale = new Vector3(2.0f, 2.0f, 2.0f);
            */

            /*
            var box = node.CreateComponent<StaticModel>();
            box.Model = cache.GetModel("Models/Box.mdl");
            //box.Material = Material.FromImage("Textures/StoneDiffuse.dds");
            */
        }

        private string text = "";
        public string Text {
            get { return text; }
            set {
                if(text.CompareTo(value) == 0) {
                    return;
                }
                text = value;
                label.Value = text;

                int textWidth = (int)Math.Ceiling(label.GetRowWidth(0));
                int textHeight = (int)Math.Ceiling(label.RowHeight);
                textWidth = Math.Max(textWidth + 40, 64);
                textHeight = Math.Max(textHeight + 20, 64);
                background.Size = new IntVector2(textWidth, textHeight);
                panel.Root.Size = new IntVector2(textWidth, textHeight);

                var billboard = billboards.GetBillboardSafe(0);
                billboard.Size = new Vector2(0.001f * textWidth, 0.001f * textHeight);
                billboard.Enabled = true;
                billboards.Commit();
            }
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

        public Color Color {
            get {
                return label.GetColor(Corner.BottomLeft);
            }
            set {
                label.SetColor(value);
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
