using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Audio;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace BodyAR {
    class ModelManager : Component {
        public static ModelManager Inst { get; private set; }

        public ModelManager() {
            Inst = this;
        }

        public override void OnAttachedToNode(Node node) {
            base.OnAttachedToNode(node);
        }

        public void UpdateModel(string id, string model, bool show) {
            var modelNode = Node.GetChild(id);
            if(modelNode == null) {
                modelNode = Node.CreateChild(id);
            }

            var sm = modelNode.GetOrCreateComponent<StaticModel>();
            if(sm.Model == null || sm.Model.Name != model) {
                sm.Model = Application.ResourceCache.GetModel(model);
                var material = Application.ResourceCache.GetMaterial("Materials/TintedModel.xml");
                material.SetShaderParameter("MatDiffColor", Color.White);
                sm.Material = material;
            }

            modelNode.Enabled = show;
        }

        public void UpdateModelTransform(string id, Vector3 position, Quaternion rotation, float scale) {
            var modelNode = Node.GetChild(id);
            if(modelNode == null) {
                return;
            }

            modelNode.Position = position;
            modelNode.Rotation = rotation;
            modelNode.SetScale(scale);
        }

        public void UpdateModelTint(string id, Color tint) {
            var modelNode = Node.GetChild(id);
            if(modelNode == null) {
                return;
            }

            var sm = modelNode.GetOrCreateComponent<StaticModel>();
            sm.Material.SetShaderParameter("MatDiffColor", tint);
        }

        public async Task LoadModel(string modelID, Uri uri) {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            var buffer = await response.Content.ReadAsBufferAsync();
            var memBuffer = new MemoryBuffer(buffer.ToArray());
            Model model = new Model();
            if(!model.Load(memBuffer)) {
                return;
            }

            model.Name = modelID;
            Application.ResourceCache.AddManualResource(model);
        }
    }
}

