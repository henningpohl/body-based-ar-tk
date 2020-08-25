using ChakraHost.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Data.Json;
using System.Linq;
using Urho;
using Windows.Graphics.Imaging;
using Windows.Media.FaceAnalysis;

namespace BodyAR {
    class PoseTracker {

        private List<JavaScriptValue> callbacks = new List<JavaScriptValue>();
        private bool busy = false;

        public static PoseTracker Inst {
            get; private set;
        }

        public PoseTracker() {
            Inst = this;
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

            RecognitionServer.Inst.RecognizePose(frame.bitmap, (x) => {
                var o = JsonObject.Parse(x);
                var detectedPoses = o["PoseDetection"].GetArray();

                ProjectRuntime.Inst.DispatchRuntimeCode(() => {
                    var poses = JavaScriptValue.CreateArray(0);
                    var pushFunc = poses.GetProperty(JavaScriptPropertyId.FromString("push"));

                    foreach(var item in detectedPoses) {
                        var pose = item.GetObject();
                        var id = pose["PoseID"].GetNumber();
                        var score = pose["PoseScore"].GetNumber();
                        var keypoints = pose["Keypoints"].GetArray();
                        var newKeypoints = new JsonObject();
                        foreach(var jtem in keypoints) {
                            var keypoint = jtem.GetObject().First();
                            var defaultPosition = new JsonArray();
                            defaultPosition.Add(JsonValue.CreateNumberValue(0));
                            defaultPosition.Add(JsonValue.CreateNumberValue(0));
                            defaultPosition.Add(JsonValue.CreateNumberValue(0));
                            var position = keypoint.Value.GetObject().GetNamedArray("Position", defaultPosition);
                            var pos = GetEstimatedPositionFromPosition(new Vector3((float)position.GetNumberAt(0), (float)position.GetNumberAt(1), (float)position.GetNumberAt(2)), frame.bitmap);
                            //var pos = GetEstimatedPositionFromPosition(new Vector3(1, 1, 1), frame.bitmap);
                            var newKeypoint = new JsonObject();
                            var newPosition = new JsonArray();
                            newPosition.Add(JsonValue.CreateNumberValue(pos.X));
                            newPosition.Add(JsonValue.CreateNumberValue(pos.Y));
                            newPosition.Add(JsonValue.CreateNumberValue(pos.Z));
                            newKeypoint.Add("position", newPosition);
                            newKeypoint.Add("score", JsonValue.CreateNumberValue(keypoint.Value.GetObject().GetNamedNumber("Score", 0.5)));
                            //newKeypoint.Add("score", JsonValue.CreateNumberValue(0));
                            newKeypoints.Add(keypoint.Key, newKeypoint);

                        }
                        var jsPose = JavaScriptContext.RunScript($"new Pose({id}, {score}, {newKeypoints.Stringify()});");
                        pushFunc.CallFunction(poses, jsPose);
                    }


                    foreach(var callback in callbacks) {
                        callback.CallFunction(callback, poses);
                    }
                });

                busy = false;
            });
        }

        private Vector3 GetEstimatedPositionFromPosition(Vector3 position, SoftwareBitmap src)
        {
            float cx = position.X / src.PixelWidth;
            float cy = position.Y / src.PixelHeight;

            // TODO: introduce a clever way to map face size to face distance

            int viewX = (int)(cx * Application.Current.Graphics.Width);
            int viewY = (int)(cy * Application.Current.Graphics.Height);

            //return new Vector3(cx, cy, 0);
            var viewport = Application.Current.Renderer.GetViewport(0);
            return viewport.ScreenToWorldPoint(viewX, viewY, 1.0f);
        }

    }
}