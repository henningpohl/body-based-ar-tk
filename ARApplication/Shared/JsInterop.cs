using ChakraHost.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Windows.Data.Json;
using Windows.Graphics.Imaging;

namespace BodyAR {
    class JsInterop {


        #region Plotting output

        private Dictionary<string, Label> labelDict = new Dictionary<string, Label>();

        [JsRuntime.JsExport(JavaScriptMethodName = "updateLabel")]
        public JavaScriptValue UpdateLabel(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount != 6) {
                return JavaScriptValue.Invalid;
            }

            var labelId = arguments[1].ToString();

            Vector3? position = arguments[2].IsUndefined() ? (Vector3?)null : arguments[2].ToVector3();           
            var color = arguments[3].ToColor();
            var text = arguments[4].IsUndefined() ? null : arguments[4].ToString();
            var show = arguments[5].ToBoolean();
            if(text == null || position.HasValue == false) {
                show = false;
            }

            if(!labelDict.ContainsKey(labelId)) {
                var label = new Label("");
                labelDict[labelId] = label;
            }

            if(text != null) {
                labelDict[labelId].Text = text;
            }
            if(position.HasValue) {
                labelDict[labelId].Position = position.Value;
            }
            labelDict[labelId].Visible = show;
            labelDict[labelId].Color = color;
            return JavaScriptValue.Invalid;
        }

        private Dictionary<string, Sprite> spriteDict = new Dictionary<string, Sprite>();

        [JsRuntime.JsExport(JavaScriptMethodName = "updateSprite")]
        public JavaScriptValue UpdateSprite(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount != 5) {
                return JavaScriptValue.Invalid;
            }

            var spriteId = arguments[1].ToString();

            Vector3? position = arguments[2].IsUndefined() ? Vector3.Zero : arguments[2].ToVector3();
            var image = arguments[3].IsUndefined() ? null : arguments[3].ToString();
            var show = arguments[4].ToBoolean();
            if(image == null) {
                show = false;
            }

            if(!spriteDict.ContainsKey(spriteId)) {
                var sprite = new Sprite();
                spriteDict[spriteId] = sprite;
            }

            if(position.HasValue) {
                spriteDict[spriteId].Position = position.Value;
            }
            if(image != null) {
                spriteDict[spriteId].TextureID = image;
            }
            spriteDict[spriteId].Visible = show;

            return JavaScriptValue.Invalid;
        }

        private Dictionary<string, Bargraph> bargraphDict = new Dictionary<string, Bargraph>();

        [JsRuntime.JsExport(JavaScriptMethodName = "updateBarplot")]
        public JavaScriptValue UpdateBarplot(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount != 4) {
                return JavaScriptValue.Invalid;
            }

            var graphID = arguments[1].ToString();
            var position = arguments[2].ToVector3();

            var plotCount = arguments[3].GetProperty(JavaScriptPropertyId.FromString("length")).ConvertToNumber().ToInt32();
            var plotData = new List<Bargraph.BarData>(plotCount);
            for(int i = 0; i < plotCount; ++i) {
                var row = arguments[3].GetIndexedProperty(JavaScriptValue.FromInt32(i));

                if(!row.Has("value")) {
                    continue;
                }

                var label = row.Has("label") ? row.Get("label").ToString() : null;
                var value = (float)row.Get("value").ConvertToNumber().ToDouble();
                var color = row.Has("color") ? row.Get("color").ToColor() : (Color?)null;

                plotData.Add(new Bargraph.BarData() {
                    Label = label,
                    Value = value,
                    Color = color
                });
            }

            if(bargraphDict.ContainsKey(graphID)) {
                bargraphDict[graphID].Update(plotData);
                bargraphDict[graphID].Position = position;
            } else {
                var graph = new Bargraph();
                graph.Update(plotData);
                graph.Position = position;
                bargraphDict[graphID] = graph;
            }

            return JavaScriptValue.Invalid;
        }

        private Dictionary<string, Path> pathDict = new Dictionary<string, Path>();

        [JsRuntime.JsExport(JavaScriptMethodName = "updatePath")]
        public JavaScriptValue UpdatePath(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount != 4) {
                return JavaScriptValue.Invalid;
            }

            var pathID = arguments[1].ToString();
            var color = arguments[3].ToColor();

            var pathPoints = arguments[2].Length();
            if(!pathPoints.HasValue) {
                return JavaScriptValue.Invalid;
            }

            var pathData = new List<Vector3>(pathPoints.Value);
            for(int i = 0; i < pathPoints.Value; ++i) {
                var point = arguments[2].Get(i).ToVector3();
                pathData.Add(point);
            }

            if(pathDict.ContainsKey(pathID)) {
                pathDict[pathID].Update(pathData);
                pathDict[pathID].Color = color;
            } else {
                var path = Path.Create();
                path.Update(pathData);
                path.Color = color;
                pathDict[pathID] = path;
            }

            return JavaScriptValue.Invalid;
        }

        #endregion

        #region Tracking and recognizing

        [JsRuntime.JsExport(JavaScriptMethodName = "registerUserTracker")]
        public JavaScriptValue RegisterUserTracker(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount != 2) {
                return JavaScriptValue.Invalid;
            }

            UserTracker.Inst.AddCallback(arguments[1]);

            return JavaScriptValue.Invalid;
        }

        [JsRuntime.JsExport(JavaScriptMethodName = "registerPoseTracker")]
        public JavaScriptValue RegisterPoseTracker(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount != 2) {
                return JavaScriptValue.Invalid;
            }

            PoseTracker.Inst.AddCallback(arguments[1]);

            return JavaScriptValue.Invalid;
        }

        [JsRuntime.JsExport(JavaScriptMethodName = "registerFaceTracker")]
        public JavaScriptValue RegisterFaceTracker(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount != 2) {
                return JavaScriptValue.Invalid;
            }

            FaceTracker.Inst.AddCallback(arguments[1]);

            return JavaScriptValue.Invalid;
        }

        private Dictionary<string, FaceRecognizer> faceRecognizers = new Dictionary<string, FaceRecognizer>();

        [JsRuntime.JsExport(JavaScriptMethodName = "registerFace")]
        public JavaScriptValue RegisterFace(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount != 4) {
                return JavaScriptValue.Invalid;
            }

            var recoID = arguments[1].ToString();
            var imgPath = arguments[2].ToString();
            var name = arguments[3].ToString();

            if(!faceRecognizers.ContainsKey(recoID)) {
                faceRecognizers[recoID] = new FaceRecognizer();
            }

            var imgUrl = new UriBuilder("http", Configuration.PROJECT_SERVER, Configuration.PROJECT_SERVER_PORT, imgPath).Uri;
            faceRecognizers[recoID].RegisterFace(imgUrl, name);

            return JavaScriptValue.Invalid;
        }

        [JsRuntime.JsExport(JavaScriptMethodName = "recognizeFaces")]
        public JavaScriptValue RecognizeFaces(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount != 4) {
                return JavaScriptValue.Invalid;
            }

            var recoID = arguments[1].ToString();
            if(!faceRecognizers.ContainsKey(recoID)) {
                faceRecognizers[recoID] = new FaceRecognizer();
            }

            var callback = arguments[3];
            if(callback.IsUndefined()) {
                return JavaScriptValue.Invalid;
            }

            var faces = arguments[2];
            var faceCount = faces.Length();
            if(!faceCount.HasValue) {
                return JavaScriptValue.Invalid;
            }

            faceRecognizers[recoID].RecognizeFaces(faces, callback);

            return JavaScriptValue.Invalid;
        }

        private EmotionRecognizer emotionRecognizer;

        [JsRuntime.JsExport(JavaScriptMethodName = "getEmotionForFaces")]
        public JavaScriptValue GetEmotionForFaces(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount != 3) {
                return JavaScriptValue.Invalid;
            }

            if(emotionRecognizer == null) {
                emotionRecognizer = new EmotionRecognizer();
            }

            var callback = arguments[2];
            if(callback.IsUndefined()) {
                return JavaScriptValue.Invalid;
            }

            var faces = arguments[1];
            var faceCount = faces.Length();
            if(!faceCount.HasValue) {
                return JavaScriptValue.Invalid;
            } else if(faceCount.Value == 0) {
                callback.CallFunction(callback, JavaScriptValue.CreateArray(0));
                return JavaScriptValue.Invalid;
            }

            emotionRecognizer.GetEmotionForFaces(faces, callback);

            return JavaScriptValue.Invalid;
        }
        #endregion

        #region Sound functions
        [JsRuntime.JsExport(JavaScriptMethodName = "loadSound")]
        public JavaScriptValue LoadSound(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount != 3) {
                return JavaScriptValue.Invalid;
            }

            string soundID = arguments[1].ToString();
            string soundPath = arguments[2].ToString();

            Uri soundURL = new UriBuilder("http", Configuration.PROJECT_SERVER, Configuration.PROJECT_SERVER_PORT, soundPath).Uri;
            SoundManager.Inst.LoadSound(soundID, soundURL);

            return JavaScriptValue.Invalid;
        }

        [JsRuntime.JsExport(JavaScriptMethodName = "playSound")]
        public JavaScriptValue PlaySound(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount < 2) {
                return JavaScriptValue.Invalid;
            }

            string soundID = arguments[1].ToString();
            if(argumentCount > 2) {
                Vector3 position = arguments[2].ToVector3();
                SoundManager.Inst.PlaySpatialSound(soundID, position);
            } else {
                SoundManager.Inst.PlaySound(soundID);
            }

            return JavaScriptValue.Invalid;
        }

        [JsRuntime.JsExport(JavaScriptMethodName = "speakText")]
        public JavaScriptValue SpeakText(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount < 2) {
                return JavaScriptValue.Invalid;
            }

            var text = arguments[1].ToString();
            if(argumentCount == 3 && arguments[2].IsValid) {
                var position = arguments[2].Get(0).ToVector3();
                SoundManager.Inst.SayText(text, position);
            } else {
                SoundManager.Inst.SayText(text);
            }

            return JavaScriptValue.Invalid;
        }
        #endregion

        #region Images

        [JsRuntime.JsExport(JavaScriptMethodName = "loadImage")]
        public JavaScriptValue LoadImage(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount != 3) {
                return JavaScriptValue.Invalid;
            }

            string imageID = arguments[1].ToString();
            string imagePath = arguments[2].ToString();
            
            Uri imageURL = new UriBuilder("http", Configuration.PROJECT_SERVER, Configuration.PROJECT_SERVER_PORT, imagePath).Uri;
            ImageManager.Inst.LoadImage(imageID, imageURL);
            
            return JavaScriptValue.Invalid;
        }

        #endregion

        #region Models

        [JsRuntime.JsExport(JavaScriptMethodName = "loadModel")]
        public JavaScriptValue LoadModel(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount != 3) {
                return JavaScriptValue.Invalid;
            }

            string modelID = arguments[1].ToString();
            string modelPath = arguments[2].ToString();

            Uri modelURL = new UriBuilder("http", Configuration.PROJECT_SERVER, Configuration.PROJECT_SERVER_PORT, modelPath).Uri;
            ModelManager.Inst.LoadModel(modelID, modelURL);

            return JavaScriptValue.Invalid;
        }

        [JsRuntime.JsExport(JavaScriptMethodName = "updateModel")]
        public JavaScriptValue UpdateModel(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount < 3) {
                return JavaScriptValue.Invalid;
            }

            string instanceID = arguments[1].ToString();
            string modelID = arguments[2].ToString();
            bool show = argumentCount > 3 ? arguments[3].ToBoolean() : true;

            ModelManager.Inst.UpdateModel(instanceID, modelID, show);

            return JavaScriptValue.Invalid;
        }

        [JsRuntime.JsExport(JavaScriptMethodName = "updateModelTint")]
        public JavaScriptValue UpdateModelTint(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount != 3) {
                return JavaScriptValue.Invalid;
            }

            string instanceID = arguments[1].ToString();
            Color tint = arguments[2].ToColor();
            ModelManager.Inst.UpdateModelTint(instanceID, tint);

            return JavaScriptValue.Invalid;
        }

        [JsRuntime.JsExport(JavaScriptMethodName = "updateModelTransform")]
        public JavaScriptValue UpdateModelTransform(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount != 5) {
                return JavaScriptValue.Invalid;
            }

            string instanceID = arguments[1].ToString();
            Vector3 position = arguments[2].ToVector3();
            Vector3 rotVector = arguments[3].ToVector3();
            var rotation = new Quaternion(rotVector.X, rotVector.Y, rotVector.Z);
            float scale = (float)arguments[4].ToDouble();

            ModelManager.Inst.UpdateModelTransform(instanceID, position, rotation, scale);

            return JavaScriptValue.Invalid;
        }

        #endregion

        #region debug helpers

        [JsRuntime.JsExport(JavaScriptMethodName = "debugOut")]
        public JavaScriptValue DebugOut(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            var funcParams = from x in arguments.Skip(1) select x.ConvertToString().ToString();
            string debugString = string.Join(" ", funcParams);
            System.Diagnostics.Debug.WriteLine(debugString);

            return JavaScriptValue.Invalid;
        }

        [JsRuntime.JsExport(JavaScriptMethodName = "sendToEditor")]
        public JavaScriptValue SendToEditor(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount != 3) {
                return JavaScriptValue.Invalid;
            }

            var channel = arguments[1].ToString();
            var message = arguments[2].ToString();

            var app = ProjectRuntime.Inst;
            app.client.Emit(channel, JsonValue.CreateStringValue(message));

            return JavaScriptValue.Invalid;
        }

        #endregion

        public void Clear() {
            labelDict.Clear();
            bargraphDict.Clear();
            pathDict.Clear();
            faceRecognizers.Clear();
        }
    }
}
