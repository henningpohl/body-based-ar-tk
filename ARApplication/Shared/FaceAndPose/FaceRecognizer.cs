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
    class FaceRecognizer {

        private HttpClient http;
        private FaceDetector faceDetector;
        private RecognitionServer server;

        public FaceRecognizer() {
            http = new HttpClient();
            server = RecognitionServer.Inst;
        }

        public async Task RegisterFace(Uri uri, string name) {
            if(faceDetector == null) {
                faceDetector = await FaceDetector.CreateAsync();
            }

            var stream = await http.GetStreamAsync(uri);
            var memStream = new MemoryStream();
            await stream.CopyToAsync(memStream);
            memStream.Position = 0;



            var decoder = await BitmapDecoder.CreateAsync(memStream.AsRandomAccessStream());
            var bitmap = await decoder.GetSoftwareBitmapAsync();

            /*
            var faceBounds = await FindFace(bitmap);
            if(!faceBounds.HasValue) {
                System.Diagnostics.Debug.WriteLine("More than or less than one face found in training image");
                return;
            }
            */

            // https://forums.xamarin.com/discussion/63447/cannot-crop-or-resize-images-on-windows-universal-uwp
            var dstStream = new InMemoryRandomAccessStream();
            //var encoder = await BitmapEncoder.CreateForTranscodingAsync(dstStream, decoder);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, dstStream);
            //encoder.BitmapTransform.Bounds = faceBounds.Value;
            encoder.SetSoftwareBitmap(bitmap);
            await encoder.FlushAsync();

            dstStream.Seek(0);
            var imgBuffer = new byte[dstStream.Size];
            await dstStream.ReadAsync(imgBuffer.AsBuffer(), (uint)dstStream.Size, InputStreamOptions.None);

            /*
            // uncomment to dump files to disk for debug
            var file = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync($"test-{name}.jpg", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            using(var outStream = await file.OpenStreamForWriteAsync()) {
                await outStream.WriteAsync(imgBuffer, 0, imgBuffer.Length);
            }
            */

            server.SaveFace(imgBuffer, name, true, null);
        }

        public void RecognizeFaces(JavaScriptValue faces, JavaScriptValue callback) {
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

            server.RecognizeFaces(frame.bitmap, boundList, (s) => {
                JsonObject json;
                if(!JsonObject.TryParse(s, out json)) {
                    ProjectRuntime.Inst.DispatchRuntimeCode(() => {
                        for(int i = 0; i < faces.Length().Value; ++i) {
                            faces.Get(i).SetProperty(JavaScriptPropertyId.FromString("name"), JavaScriptValue.FromString("Unknown"), true);
                        }
                        callback.CallFunction(callback, faces);
                        callback.Release();
                        faces.Release();
                    });
                    return;
                }

                var responses = json.GetNamedArray("ResponsePerFace");
                var names = new List<string>();
                for(int i = 0; i < responses.Count; ++i) {
                    var faceResponse = responses.GetObjectAt((uint)i);
                    names.Add(faceResponse.GetNamedString("FaceRecognition"));
                }

                ProjectRuntime.Inst.DispatchRuntimeCode(() => {
                    for(int i = 0; i < faces.Length().Value; ++i) {
                        faces.Get(i).SetProperty(JavaScriptPropertyId.FromString("name"), JavaScriptValue.FromString(names[i]), true);
                    }
                    callback.CallFunction(callback, faces);
                    callback.Release();
                    faces.Release();
                });
            });
        }

        private async Task<BitmapBounds?> FindFace(SoftwareBitmap bitmap) {
            if(!FaceDetector.IsBitmapPixelFormatSupported(bitmap.BitmapPixelFormat)) {
                bitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Gray8);
            }

            var faces = await faceDetector.DetectFacesAsync(bitmap);
            if(faces.Count != 1) {
                return null;
            }

            return faces[0].FaceBox;
        }
    }
}