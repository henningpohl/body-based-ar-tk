using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Gui;
using Urho.Resources;

namespace BodyAR {
    class Gui {
        private Text statusText;
        private Text debugText;
        private DebugHud debugHud;

        public string Status {
            set {
                statusText.Value = value;
            }
        }

        public Gui() {
            var cache = Application.Current.ResourceCache;

            statusText = Application.Current.UI.Root.CreateText();
            statusText.HorizontalAlignment = HorizontalAlignment.Center;
            statusText.VerticalAlignment = VerticalAlignment.Center;
            statusText.SetFont(cache.GetFont("Fonts/OpenSans-Bold.ttf"), 24);

            debugText = Application.Current.UI.Root.CreateText();
            debugText.HorizontalAlignment = HorizontalAlignment.Left;
            debugText.VerticalAlignment = VerticalAlignment.Bottom;
            debugText.SetFont(cache.GetFont("Fonts/OpenSans-Light.ttf"), 12);

            debugHud = Application.Current.Engine.CreateDebugHud();
            debugHud.DefaultStyle = cache.GetXmlFile("UI/DefaultStyle.xml");

            Application.Current.Input.Enabled = true;
            Application.Current.Input.KeyDown += (e) => {
                switch(e.Key) {
                    case Key.F2:
                        debugHud.ToggleAll();
                        break;
                }
            };
        }

        public void ShowDebugMessage(string msg) {
            debugText.Value = msg;
        }
    }
}
/*
var image = new BorderImage() {
    Texture = cache.GetTexture2D("Textures/UrhoIcon.png"),
    ImageRect = new IntRect(0, 0, 48, 48),
    Size = new IntVector2(100, 100),
    Position = new IntVector2(100, 100)
};
//UI.Root.AddChild(image);
*/

/*
var uiNode = scene.CreateChild();
uiNode.Position = new Vector3(0.0f, 2.0f, 0.0f);
uiNode.Scale = new Vector3(2.0f, 2.0f, 2.0f);
//uiNode.Rotation = new Quaternion(Vector3.Left, 50.0f);
var box = uiNode.CreateComponent<StaticModel>();
box.Model = cache.GetModel("Models/Box.mdl");
//box.Material = Material.FromImage("Textures/StoneDiffuse.dds");

var uicomp = uiNode.CreateComponent<UIComponent>();
uicomp.Material.SetTechnique(0, cache.GetTechnique("Techniques/DiffUnlitAlpha.xml"));
var texRoot = uicomp.Root;
texRoot.Size = new IntVector2(512, 512);
texRoot.AddChild(image);
*/
