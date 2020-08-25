using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Urho2D;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace BodyAR {
    class ImageManager : Component {
        public static ImageManager Inst { get; private set; }

        public ImageManager() {
            Inst = this;
        }

        public override void OnAttachedToNode(Node node) {
            base.OnAttachedToNode(node);
        }

        public async Task LoadImage(string imageID, Uri uri) {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            var buffer = await response.Content.ReadAsBufferAsync();
            var memBuffer = new MemoryBuffer(buffer.ToArray());
    

            Texture2D texture = new Texture2D();
            if(!texture.Load(memBuffer)) {
                return;
            }
            texture.Name = imageID;
            Application.ResourceCache.AddManualResource(texture);
        }
    }
}

