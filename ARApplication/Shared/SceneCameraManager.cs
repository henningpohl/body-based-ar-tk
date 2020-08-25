using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Urho2D;
using Windows.Graphics.Imaging;
using Windows.Media.FaceAnalysis;

namespace BodyAR {
    [ComImport]
    [Guid("5b0d3235-4dba-4d44-865e-8f1d0e4fd04d")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

    class SceneCameraManager {

        private ISceneCamera camera;
        private Texture2D cameraTexture;
        private RenderPathCommand cameraRenderPathCommand;

        public int CameraWidth => camera.Width;
        public int CameraHeight => camera.Height;
        public Matrix4 CameraProjection => camera.ProjectionMatrix;

        private FaceTracker faceTracker;
        private PoseTracker poseTracker;

        private Dictionary<int, FrameData> frameCache = new Dictionary<int, FrameData>();
        private int maxCacheId = 0;

        private SceneCameraManager() {
            faceTracker = new FaceTracker();
            poseTracker = new PoseTracker();
        }

        private static SceneCameraManager _inst;
        public static SceneCameraManager Inst {
            get {
                if(_inst == null) {
                    _inst = new SceneCameraManager();
                }
                return _inst;
            }
        }

        public async Task Init(ISceneCamera camera) {
            this.camera = camera;
            await camera?.InitAsync();
            if(camera.ShouldRender) {
                SetupCameraPreview();
            }

            camera.FrameReady += faceTracker.ProcessFrame;
            camera.FrameReady += poseTracker.ProcessFrame;
        }


        public void Start() {
            camera?.StartCameraAsync();
        }

        public void Stop() {
            camera?.StopCameraAsync();
        }

        public int AddFrameToCache(FrameData frame) {
            var key = maxCacheId++;
            frameCache.Add(key, frame);
            return key;
        }

        public FrameData GetFrameFromCache(int key) {
            FrameData frame;
            if(frameCache.TryGetValue(key, out frame)) {
                return frame;
            } else {
                return null;
            }
        }

        public void RemoveFrameFromCache(int key) {
            // TODO: fix shitty memory overflow bug
            for(int i = 0; i <= key; ++i) {
                if(frameCache.ContainsKey(i)) {
                    frameCache.Remove(i);
                }
            }
        }

        // https://github.com/xamarin/urho/blob/master/Extensions/Urho.Extensions.Droid.ARCore/ARCore.cs   
        // https://github.com/xamarin/urho/blob/master/Bindings/Portable/SharpReality/YuvVideo.cs
        // https://stackoverflow.com/questions/29947225/access-preview-frame-from-mediacapture
        private void SetupCameraPreview() {
            if(camera == null) {
                return;
            }

            cameraTexture = new Texture2D();
            cameraTexture.SetNumLevels(1);
            cameraTexture.FilterMode = TextureFilterMode.Bilinear;
            cameraTexture.SetAddressMode(TextureCoordinate.U, TextureAddressMode.Clamp);
            cameraTexture.SetAddressMode(TextureCoordinate.V, TextureAddressMode.Clamp);
            cameraTexture.Name = nameof(cameraTexture);
            Application.Current.ResourceCache.AddManualResource(cameraTexture);

            cameraRenderPathCommand = new RenderPathCommand(RenderCommandType.Quad);
            cameraRenderPathCommand.VertexShaderName = (UrhoString)"VideoRect";
            cameraRenderPathCommand.PixelShaderName = (UrhoString)"VideoRect";
            cameraRenderPathCommand.SetOutput(0, "viewport");
            cameraRenderPathCommand.SetTextureName(TextureUnit.Diffuse, cameraTexture.Name);
            var viewport = Application.Current.Renderer.GetViewport(0);
            viewport.RenderPath.InsertCommand(1, cameraRenderPathCommand);

            camera.FrameReady += async (frame) => {
                if(cameraTexture.Width != frame.bitmap.PixelWidth || cameraTexture.Height != frame.bitmap.PixelHeight) {
                    cameraTexture.SetSize(frame.bitmap.PixelWidth, frame.bitmap.PixelHeight, Graphics.RGBAFormat, TextureUsage.Dynamic);
                }

                unsafe {
                    using(var buffer = frame.bitmap.LockBuffer(Windows.Graphics.Imaging.BitmapBufferAccessMode.Read)) {
                        using(var bufferRef = buffer.CreateReference()) {
                            byte* data;
                            uint capacity;
                            ((IMemoryBufferByteAccess)bufferRef).GetBuffer(out data, out capacity);

                            cameraTexture.SetData(0, 0, 0, frame.bitmap.PixelWidth, frame.bitmap.PixelHeight, data);
                        }
                    }
                }
            };
        }
    }
}
