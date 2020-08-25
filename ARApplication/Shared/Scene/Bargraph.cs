using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Gui;

namespace BodyAR {
    // Also check https://github.com/xamarin/urho-samples/blob/master/FeatureSamples/Core/41_Charts/Charts.cs for alternative
    class Bargraph : Component {

        public struct BarData {
            public string Label;
            public Color? Color;
            public float Value;
        };

        // From default seaborn.color_palette()
        private static Color[] colorCycle = new Color[] {
            Color.FromHex("1F77B4"),
            Color.FromHex("FF7F0E"),
            Color.FromHex("2CA02C"),
            Color.FromHex("D62728"),
            Color.FromHex("9467BD"),
            Color.FromHex("8C564B"),
            Color.FromHex("E377C2"),
            Color.FromHex("7F7F7F"),
            Color.FromHex("BCBD22"),
            Color.FromHex("17BECF")
        };

        
        private UIElement graphRoot;
        private UIComponent panel;
        private BillboardSet billboards;

        private List<BarData> data = new List<BarData>();

        private Texture barTex;
        private Font barFont;

        private const int HEIGHT = 45;
        private const int MAX_BAR_WIDTH = 250;
        private const int LABEL_GAP = 5;
        private int labelWidth = 50;

        public Bargraph() {
            var node = FloatManager.Inst.Node.CreateChild();
            node.Position = new Vector3(0, 0, 0);

            node.AddComponent(this);
            Init();
        }

        private void Init() {
            var cache = Application.ResourceCache;
            barTex = cache.GetTexture2D("Textures/Bargraph.png");
            barFont = cache.GetFont("Fonts/OpenSans-SemiBold.ttf");

            graphRoot = new UIElement();
            graphRoot.SetStyleAuto(null);
            graphRoot.LayoutMode = LayoutMode.Free;

            panel = Node.CreateComponent<UIComponent>();
            panel.Material.SetTechnique(0, cache.GetTechnique("Techniques/DiffUnlitAlpha.xml"));
            panel.Root.LayoutMode = LayoutMode.Free;
            panel.Root.HorizontalAlignment = HorizontalAlignment.Center;
            panel.Root.VerticalAlignment = VerticalAlignment.Center;
            panel.Root.AddChild(graphRoot);

            billboards = Node.CreateComponent<BillboardSet>();
            billboards.NumBillboards = 1;
            billboards.Material = panel.Material;

            var billboard = billboards.GetBillboardSafe(0);
            billboard.Enabled = false;
            billboards.Commit();
        }


        public void Update(IEnumerable<BarData> newData) {
            if(newData.Count() != data.Count) {
                graphRoot.Size = new IntVector2(MAX_BAR_WIDTH + labelWidth, newData.Count() * HEIGHT);

                graphRoot.RemoveAllChildren();
                for(int i = 0; i < newData.Count(); ++i) {
                    var row = new UIElement();

                    var label = new Text() {
                        TextAlignment = HorizontalAlignment.Right,
                        HorizontalAlignment = HorizontalAlignment.Right
                    };
                    label.SetColor(new Color(1.0f, 1.0f, 1.0f));
                    label.SetFont(barFont, 22);
                    label.Position = new IntVector2(labelWidth, i * HEIGHT);
                    label.Width = labelWidth;
                    row.AddChild(label);

                    var bar = new BorderImage() {
                        Texture = barTex,
                        ImageRect = new IntRect(0, 0, 32, 32),
                        Border = new IntRect(6, 9, 6, 9),

                        MinSize = new IntVector2(20, 20),
                        BlendMode = BlendMode.Alpha
                    };
                    bar.Position = new IntVector2(labelWidth + LABEL_GAP, i * HEIGHT);
                    row.AddChild(bar);

                    graphRoot.AddChild(row);
                }
            }
            data.Clear();
            data.AddRange(newData);

            var selected = data.Select(bd => bd.Value);
            // TODO: Check whether this should be in or out
            /*if (!selected.Any()) {
                return;
            }*/
            var maxValue = selected.Max();
            int maxLabelWidth = -1;
            for(int i = 0; i < data.Count; ++i) {
                var row = graphRoot.GetChild((uint)i);

                var label = row.GetChild(0) as Text;
                label.Value = data[i].Label;
                maxLabelWidth = (int)Math.Max(label.GetRowWidth(0), maxLabelWidth);

                var bar = row.GetChild(1) as BorderImage;
                if(data[i].Color.HasValue) {
                    bar.SetColor(data[i].Color.Value);
                } else {
                    bar.SetColor(colorCycle[i % colorCycle.Length]);
                }
                bar.Size = new IntVector2((int)(MAX_BAR_WIDTH * (data[i].Value / maxValue)), HEIGHT);
            }
            
            if(maxLabelWidth != labelWidth) {
                labelWidth = maxLabelWidth;
                for(int i = 0; i < data.Count; ++i) {
                    var row = graphRoot.GetChild((uint)i);
                    var label = row.GetChild(0) as Text;
                    var bar = row.GetChild(1) as BorderImage;

                    label.Position = new IntVector2(labelWidth, i * HEIGHT);
                    label.Width = labelWidth;
                    bar.Position = new IntVector2(labelWidth + LABEL_GAP, i * HEIGHT);
                }
            }

            var billboard = billboards.GetBillboardSafe(0);
            billboard.Size = new Vector2(0.001f * (MAX_BAR_WIDTH + labelWidth), 0.002f * (HEIGHT * data.Count));
            billboard.Enabled = true;
            billboards.Commit();
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
