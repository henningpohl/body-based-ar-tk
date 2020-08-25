using ChakraHost.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Graphics.Imaging;
using Windows.Media.FaceAnalysis;
using Windows.Storage.Streams;

namespace BodyAR {
    class EmotionRecognizer {
        private RecognitionServer server;

        public EmotionRecognizer() {
            server = RecognitionServer.Inst;
        }

        public void GetEmotionForFaces(JavaScriptValue faces, JavaScriptValue callback) {
            if(RecognitionServer.Inst.IsBusy) {
                return;
            }

            var boundList = new List<BitmapBounds>();
            for(int i = 0; i < faces.Length().Value; ++i) {
                var jsBounds = faces.Get(i).Get("bounds");
                var bounds = new BitmapBounds() {
                    X = (uint)jsBounds.Get("x").ToInt32(),
                    Y = (uint)jsBounds.Get("y").ToInt32(),
                    Width = (uint)jsBounds.Get("width").ToInt32(),
                    Height = (uint)jsBounds.Get("height").ToInt32()
                };
                boundList.Add(bounds);
            }

            int frameID = faces.Get(0).Get("frame").Get("id").ToInt32();
            var frame = SceneCameraManager.Inst.GetFrameFromCache(frameID);

            callback.AddRef();
            faces.AddRef();

            server.RecognizeEmotions(frame.bitmap, boundList, (s) => {
                JsonObject json;
                if(!JsonObject.TryParse(s, out json)) {
                    ProjectRuntime.Inst.DispatchRuntimeCode(() => {
                        var emotions = JavaScriptValue.CreateArray(0);
                        var pushFunc = emotions.GetProperty(JavaScriptPropertyId.FromString("push"));
                        for(var i = 0; i < faces.Length().Value; ++i) {
                            pushFunc.CallFunction(faces, JavaScriptValue.FromString("Unknown"));
                        }
                        callback.CallFunction(callback, faces);
                        callback.Release();
                        faces.Release();
                    });
                    return;
                }

                var responses = json.GetNamedArray("ResponsePerFace");
                ProjectRuntime.Inst.DispatchRuntimeCode(() => {
                    var emotions = JavaScriptValue.CreateArray(0);
                    var pushFunc = emotions.GetProperty(JavaScriptPropertyId.FromString("push"));
                    for(int i = 0; i < faces.Length().Value; ++i) {
                        var emotion = responses.GetObjectAt((uint)i).GetNamedString("Emotion");
                        pushFunc.CallFunction(emotions, JavaScriptValue.FromString(emotion));
                    }
                    callback.CallFunction(callback, emotions);
                    callback.Release();
                    faces.Release();
                });
            });
        }        
    }
}