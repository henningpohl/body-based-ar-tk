using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Networking;
using Windows.Networking.Sockets;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using System.Text;
using Windows.Data.Json;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace BodyAR {
    class RecognitionServer {
        private bool connected = false;
        private string hostname;
        private int port;

        private StreamSocket serverSocket;
        private Stream outputStream;

        public bool waitingForReply = false;

        public event EventHandler<String> responseReceived;

        private struct Request {
            public byte[] Message;
            public Action<string> ResponseHandler;
        }
        private Queue<Request> requestQueue = new Queue<Request>();

        public bool IsBusy => requestQueue.Count > 0;

        private static RecognitionServer inst;
        public static RecognitionServer Inst {
            get {
                if(inst == null) {
                    inst = new RecognitionServer();
                    inst.Initialize(Configuration.RECOGNITION_SERVER, Configuration.RECOGNITION_SERVER_PORT);
                }
                return inst;
            }
        }

        private RecognitionServer() { }

        private async Task Connect() {
            try {
                serverSocket = new StreamSocket();
                var hostName = new HostName(hostname);
                await serverSocket.ConnectAsync(hostName, port.ToString());

                Debug.WriteLine("Client Connected");
                connected = true;
            } catch(Exception ex) {
                Debug.WriteLine(ex.Message);
            }

            if(serverSocket != null) {
                //inputStream = new StreamReader(serverSocket.InputStream.AsStreamForRead());
                outputStream = serverSocket.OutputStream.AsStreamForWrite();
                //inputStream = serverSocket.InputStream.AsStreamForRead();
                Debug.WriteLine("Streams initialized.");
            }
        }

        public void Initialize(string hostname, int port = 43002) {
            this.hostname = hostname;
            this.port = port;
            Task.Run(ServerTask);
        }

        private async Task ServerTask() {
            while(true) {
                if(!connected) {
                    Debug.WriteLine("Connecting to server");
                    await Connect();
                }

                if(requestQueue.Count == 0) {
                    await Task.Delay(20);
                    continue;
                }

                while(requestQueue.Count > 0) {
                    var request = requestQueue.Dequeue();

                    await this.outputStream.WriteAsync(request.Message, 0, request.Message.Length);
                    await this.outputStream.FlushAsync();

                    StringBuilder responseBuilder = new StringBuilder();
                    using(var reader = new DataReader(serverSocket.InputStream)) {
                        reader.InputStreamOptions = InputStreamOptions.Partial;
                        reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                        await reader.LoadAsync(1024 * 100);

                        while(reader.UnconsumedBufferLength > 0) {
                            char c = (char)reader.ReadByte();
                            if(c == '\n') {
                                break;
                            }
                            responseBuilder.Append(c);
                        }

                        reader.DetachStream();
                    }
                    string response = responseBuilder.ToString();

                    if(request.ResponseHandler != null) {
                        ThreadPool.QueueUserWorkItem((_) => {
                            request.ResponseHandler.Invoke(response);
                        });
                    }
                }
            }
        }

        public void Close() {
            connected = false;
            if(serverSocket != null)
                serverSocket.Dispose();
            if(outputStream != null)
                outputStream.Dispose();
        }

        public void SaveFace(SoftwareBitmap bitmap, string name, bool croppedImg, Action<string> responseHandler) {
            Task.Run(async () => {
                var bitmapData = await EncodedBytes(bitmap, BitmapEncoder.JpegEncoderId);
                SaveFace(bitmapData, name, croppedImg, responseHandler);
            });
        }

        public void SaveFace(byte[] bitmapData, string name, bool croppedImg, Action<string> responseHandler) {
            var message = new JsonObject();
            message.Add("CroppedImg", JsonValue.CreateBooleanValue(croppedImg));
            message.Add("RequestPose", JsonValue.CreateBooleanValue(false));

            var info = new JsonObject();
            info.Add("Type", JsonValue.CreateStringValue("Binary"));
            info.Add("Url", JsonValue.CreateStringValue(" "));
            info.Add("Length", JsonValue.CreateNumberValue(bitmapData.Length));

            var messageInfo = new JsonArray();
            messageInfo.Add(info);

            message.Add("MessageInfo", messageInfo);

            var req = new JsonObject();

            req.Add("Type", JsonValue.CreateNumberValue(8));
            req.Add("Label", JsonValue.CreateStringValue(name));
            // it's fine if the array is empty for this
            req.Add("Rect", new JsonArray());
            req.Add("ImageID", JsonValue.CreateNumberValue(0));

            var requests = new JsonArray();
            requests.Add(req);

            message.Add("RequestPerFace", requests);

            byte[] json = Encoding.ASCII.GetBytes(message.Stringify());
            byte[] jsonLength = BitConverter.GetBytes(json.Length);

            byte[] toSend = new byte[4 + json.Length + bitmapData.Length];

            jsonLength.CopyTo(toSend, 0);
            json.CopyTo(toSend, 4);
            bitmapData.CopyTo(toSend, 4 + json.Length);

            requestQueue.Enqueue(new Request() {
                Message = toSend,
                ResponseHandler = responseHandler
            });
        }


        public void RecognizePose(SoftwareBitmap bitmap, Action<string> responseHandler) {
            Task.Run(async () => {
                var bitmapData = await EncodedBytes(bitmap, BitmapEncoder.JpegEncoderId);
                RecognizePose(bitmapData, responseHandler);
            });
        }

        public void RecognizePose(byte[] bitmapData, Action<string> responseHandler) {
            var emotion = new bool[] { };
            var faceRec = new bool[] { };
            var message = AssembleMessage(new byte[][] { bitmapData }, false, emotion, faceRec, null, true);

            requestQueue.Enqueue(new Request() {
                Message = message,
                ResponseHandler = responseHandler
            });
        }

        public void RecognizeEmotions(SoftwareBitmap bitmap, List<BitmapBounds> bounds, Action<string> responseHandler) {
            Task.Run(async () => {
                var bitmapData = await EncodedBytes(bitmap, BitmapEncoder.JpegEncoderId);
                RecognizeEmotions(bitmapData, bounds, responseHandler);
            });
        }

        public void RecognizeEmotions(byte[] bitmapData, List<BitmapBounds> bounds, Action<string> responseHandler) {
            var emotion = (from b in bounds select true).ToArray();
            var faceRec = (from b in bounds select false).ToArray();
            var message = AssembleMessage(new byte[][] { bitmapData }, false, emotion, faceRec, bounds.ToArray(), false);

            requestQueue.Enqueue(new Request() {
                Message = message,
                ResponseHandler = responseHandler
            });
        }

        public void RecognizeFaces(SoftwareBitmap bitmap, List<BitmapBounds> bounds, Action<string> responseHandler) {
            Task.Run(async () => {
                var bitmapData = await EncodedBytes(bitmap, BitmapEncoder.JpegEncoderId);
                RecognizeFaces(bitmapData, bounds, responseHandler);
            });
        }

        public void RecognizeFaces(byte[] bitmapData, List<BitmapBounds> bounds, Action<string> responseHandler) {
            var emotion = (from b in bounds select false).ToArray();
            var faceRec = (from b in bounds select true).ToArray();
            var message = AssembleMessage(new byte[][] { bitmapData }, false, emotion, faceRec, bounds.ToArray(), false);

            requestQueue.Enqueue(new Request() {
                Message = message,
                ResponseHandler = responseHandler
            });
        }

        private byte[] AssembleMessage(byte[][] bitmapData, bool croppedImg, bool[] emotion, bool[] faceRec, BitmapBounds[] faceBoxes = null, bool requestPose = false) {
            uint[][] faces = null;

            if(!croppedImg && faceBoxes != null) {
                faces = new uint[faceBoxes.Length][];
                faces = this.HandleFaceBoxes(faceBoxes);
            }

            var message = new JsonObject();
            message.Add("CroppedImg", JsonValue.CreateBooleanValue(croppedImg));
            message.Add("RequestPose", JsonValue.CreateBooleanValue(requestPose));

            var messageInfo = new JsonArray();
            int imagesLength = 0;
            var requests = new JsonArray();
            for(int i = 0; i < bitmapData.Length; i++) {
                var info = new JsonObject();
                info.Add("Type", JsonValue.CreateStringValue("Binary"));
                info.Add("Url", JsonValue.CreateStringValue(" "));
                info.Add("Length", JsonValue.CreateNumberValue(bitmapData[i].Length));
                imagesLength += bitmapData[i].Length;
                messageInfo.Add(info);
            }

            message.Add("MessageInfo", messageInfo);

            // bitmapData, emotion, faceRec and faces should all be the same length
            int requestsLength = emotion.Length;
            for(int i = 0; i < requestsLength; i++) {
                var req = new JsonObject();
                int type = 0;

                if(emotion[i] && faceRec[i])
                    type = 3;
                else if(emotion[i])
                    type = 1;
                else if(faceRec[i])
                    type = 2;

                if(requestPose)
                    type = 4;

                req.Add("Type", JsonValue.CreateNumberValue(type));
                req.Add("Label", JsonValue.CreateStringValue(" "));
                var faceRectangle = new JsonArray();
                if(!croppedImg && faces != null) {
                    faceRectangle.Insert(0, JsonValue.CreateNumberValue(faces[i][0]));
                    faceRectangle.Insert(1, JsonValue.CreateNumberValue(faces[i][1]));
                    faceRectangle.Insert(2, JsonValue.CreateNumberValue(faces[i][2]));
                    faceRectangle.Insert(3, JsonValue.CreateNumberValue(faces[i][3]));
                }
                req.Add("Rect", faceRectangle);
                req.Add("ImageID", JsonValue.CreateNumberValue(i));

                requests.Add(req);
            }

            message.Add("RequestPerFace", requests);

            byte[] json = Encoding.ASCII.GetBytes(message.Stringify());
            byte[] jsonLength = BitConverter.GetBytes(json.Length);

            byte[] toSend = new byte[4 + json.Length + imagesLength];

            jsonLength.CopyTo(toSend, 0);
            json.CopyTo(toSend, 4);

            int currentPosition = 4 + json.Length;

            for(int i = 0; i < bitmapData.Length; i++) {
                bitmapData[i].CopyTo(toSend, currentPosition);
                currentPosition += bitmapData[i].Length;
            }

            return toSend;
        }


        private uint[][] HandleFaceBoxes(BitmapBounds[] faceBoxes) {
            uint[][] results = new uint[faceBoxes.Length][];

            for(int i = 0; i < faceBoxes.Length; i++) {
                results[i] = new uint[4];

                results[i][0] = faceBoxes[i].Y;
                results[i][1] = faceBoxes[i].X + faceBoxes[i].Width;
                results[i][2] = faceBoxes[i].Y + faceBoxes[i].Height;
                results[i][3] = faceBoxes[i].X;
            }

            return results;
        }

        private async Task<byte[]> EncodedBytes(SoftwareBitmap soft, Guid encoderId) {
            byte[] array = null;

            if(soft.BitmapPixelFormat != BitmapPixelFormat.Rgba8 && soft.BitmapPixelFormat != BitmapPixelFormat.Gray8) {
                soft = SoftwareBitmap.Convert(soft, BitmapPixelFormat.Rgba8);
            }

            using(var ms = new InMemoryRandomAccessStream()) {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(encoderId, ms);
                encoder.SetSoftwareBitmap(soft);

                try {
                    await encoder.FlushAsync();
                } catch(Exception ex) { return new byte[0]; }

                array = new byte[ms.Size];
                await ms.ReadAsync(array.AsBuffer(), (uint)ms.Size, InputStreamOptions.None);
            }
            return array;
        }
    }
}
