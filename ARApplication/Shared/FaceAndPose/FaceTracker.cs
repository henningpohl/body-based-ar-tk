using ChakraHost.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using Urho;
using Windows.Graphics.Imaging;
using Windows.Media.FaceAnalysis;

namespace BodyAR {
    class FaceTracker {

        private List<JavaScriptValue> callbacks = new List<JavaScriptValue>();
        private FaceDetector faceDetector;
        private bool busy = false;

        private JavaScriptObjectBeforeCollectCallback jsObjectCallback;

        public static FaceTracker Inst {
            get; private set;
        }

        public FaceTracker() {
            Inst = this;
            faceDetector = FaceDetector.CreateAsync().AsTask().Result;
            this.jsObjectCallback = OnFrameCollection;
        }

        public void AddCallback(JavaScriptValue callback) {
            if(callback.ValueType != JavaScriptValueType.Function) {
                throw new ArgumentException();
            }
            callback.AddRef();
            callbacks.Add(callback);
        }

        public async void ProcessFrame(FrameData frame) {
            if(callbacks.Count == 0 || busy) {
                return;
            } else {
                busy = true;
            }

            var bitmap = frame.bitmap;
            if(!FaceDetector.IsBitmapPixelFormatSupported(bitmap.BitmapPixelFormat)) {
                bitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Gray8);
            }

            var detectedFaces = await faceDetector.DetectFacesAsync(bitmap);

            int frameKey = -1;
            if(detectedFaces.Count > 0) {
                frameKey = SceneCameraManager.Inst.AddFrameToCache(frame);
            }

            ProjectRuntime.Inst.DispatchRuntimeCode(() => {
                var jsImg = JavaScriptValue.CreateObject();
                jsImg.SetProperty(JavaScriptPropertyId.FromString("id"), JavaScriptValue.FromInt32(frameKey), true);
                Native.JsSetObjectBeforeCollectCallback(jsImg, IntPtr.Zero, jsObjectCallback);

                var faces = JavaScriptValue.CreateArray(0);
                var pushFunc = faces.GetProperty(JavaScriptPropertyId.FromString("push"));
                foreach(var face in detectedFaces) {
                    var pos = GetEstimatedPositionFromFaceBounds(face.FaceBox, frame.bitmap);
                    var jsFace = JavaScriptContext.RunScript($"new Face(new Position({pos.X}, {pos.Y}, {pos.Z}), {0});");
                    jsFace.SetProperty(JavaScriptPropertyId.FromString("frame"), jsImg, true);
                    jsFace.SetProperty(JavaScriptPropertyId.FromString("bounds"), face.FaceBox.ToJavaScriptValue(), true);
                    pushFunc.CallFunction(faces, jsFace);
                }
                foreach(var callback in callbacks) {
                    callback.CallFunction(callback, faces);
                }
            });

            busy = false;
        }

        private void OnFrameCollection(JavaScriptValue reference, IntPtr callbackState) {
            var frameID = reference.Get("id").ToInt32();
            if(frameID != -1) {
                SceneCameraManager.Inst.RemoveFrameFromCache(frameID);
            }
        }

        private Vector3 GetEstimatedPositionFromFaceBounds(BitmapBounds bounds, SoftwareBitmap src) {
            float cx = (bounds.X + bounds.Width * 0.5f) / src.PixelWidth;
            float cy = (bounds.Y + bounds.Height * 0.5f) / src.PixelHeight;

            float size = (bounds.Width / src.PixelWidth) * (bounds.Height / src.PixelHeight);

            // TODO: introduce a clever way to map face size to face distance

            int viewX = (int)(cx * Application.Current.Graphics.Width);
            int viewY = (int)(cy * Application.Current.Graphics.Height);

            var viewport = Application.Current.Renderer.GetViewport(0);
            return viewport.ScreenToWorldPoint(viewX, viewY, 1.0f);
        }
    }
}