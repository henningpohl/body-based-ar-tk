using BodyAR;
using ChakraHost.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Web.Http;

namespace BodyAR {
    // Partial implementation per https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest
    internal class JsXmlHttpRequest {

        // https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/readyState
        internal enum ReadyStates {
            Unsent = 0,
            Opened = 1,
            Headers_Received = 2,
            Loading = 3,
            Done = 4
        }

        private JavaScriptValue objRef;
        private HttpClient client;

        private HttpRequestMessage requestMessage;
        private Task requestTask;
        /*
        private IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress> request;
        
        private CancellationTokenSource cancellationTokenSource;
        */

        private JsXmlHttpRequest(JavaScriptValue objRef) {
            this.objRef = objRef;
        }

        private ReadyStates readyState = ReadyStates.Unsent;
        internal ReadyStates ReadyState {
            get {
                return readyState;
            }
            private set {
                readyState = value;

                ProjectRuntime.Inst.DispatchRuntimeCode(() => {
                    objRef.SetProperty(JavaScriptPropertyId.FromString("readyState"), JavaScriptValue.FromInt32((int)readyState), true);
                    var callback = objRef.Get("onreadystatechanged");
                    if(callback.ValueType == JavaScriptValueType.Function) {
                        callback.CallFunction(callback);
                    }
                });
            }
        }

        private static HashSet<HttpMethod> validMethods = new HashSet<HttpMethod>() {
            HttpMethod.Get, HttpMethod.Post, HttpMethod.Head, HttpMethod.Delete, HttpMethod.Put
        };

        private HttpMethod GetHttpMethod(string method) {
            switch(method.ToUpper()) {
                case "GET":
                    return HttpMethod.Get;
                case "POST":
                    return HttpMethod.Post;
                case "HEAD":
                    return HttpMethod.Head;
                case "DELETE":
                    return HttpMethod.Delete;
                case "PUT":
                    return HttpMethod.Put;
            }
            return null;
        }

        public void Open(string method, string url) {
            var httpMethod = GetHttpMethod(method);
            if(httpMethod == null) {
                return;
            }

            client = new HttpClient();
            try {
                requestMessage = new HttpRequestMessage(httpMethod, new Uri(url));
            } catch(Exception e) {
                System.Diagnostics.Debug.WriteLine(e);
                return;
            }

            ReadyState = ReadyStates.Opened;
        }

        public void SetRequestHeader(string header, string value) {
            if(requestMessage == null) {
                return;
            }

            requestMessage.Headers.Add(header, value);
        }

        public void Send(string body) {
            if(client == null || requestMessage == null) {
                return;
            }
            if(requestTask != null) {
                return;
            }

            requestTask = Task.Run(async () => {
                try {
                    var responseHeaders = await client.SendRequestAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

                    ProjectRuntime.Inst.DispatchRuntimeCode(() => {
                        objRef.SetProperty(JavaScriptPropertyId.FromString("status"), JavaScriptValue.FromInt32((int)responseHeaders.StatusCode), true);
                        string statusText = $"{(int)responseHeaders.StatusCode} {responseHeaders.ReasonPhrase}";
                        objRef.SetProperty(JavaScriptPropertyId.FromString("statusText"), JavaScriptValue.FromString(statusText), true);
                    });
                    ReadyState = ReadyStates.Headers_Received;

                    // ReadyState = ReadyStates.Loading;  // ignore, we don't do partial responses
                    var content = await responseHeaders.Content.ReadAsStringAsync();
                    ProjectRuntime.Inst.DispatchRuntimeCode(() => {
                        objRef.SetProperty(JavaScriptPropertyId.FromString("responseType"), JavaScriptValue.FromString("text"), true);
                        objRef.SetProperty(JavaScriptPropertyId.FromString("response"), JavaScriptValue.FromString(content), true);
                        objRef.SetProperty(JavaScriptPropertyId.FromString("responseText"), JavaScriptValue.FromString(content), true);

                        var callback = objRef.Get("onload");
                        if(callback.ValueType == JavaScriptValueType.Function) {
                            callback.CallFunction(callback);
                        }
                    });
                } catch(Exception e) {
                    ProjectRuntime.Inst.DispatchRuntimeCode(() => {
                        var callback = objRef.Get("onerror");
                        if(callback.ValueType == JavaScriptValueType.Function) {
                            callback.CallFunction(callback);
                        }
                    });
                }
                ReadyState = ReadyStates.Done;
                requestMessage = null;
                requestTask = null;
            });

            /*
            cancellationTokenSource = new CancellationTokenSource();
            requestTask = Task.Run(async () => {
                var response = await request;//.AsTask(cancellationTokenSource.Token);
                //response.
                int dsa = 0;
            });
            */
        }

        public void Abort() {
            // ignore for now
            /*
            if(requestTask != null && cancellationTokenSource != null) {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
            */
        }

        private static Dictionary<IntPtr, JsXmlHttpRequest> instances = new Dictionary<IntPtr, JsXmlHttpRequest>();

        public static JavaScriptValue JsConstructor(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(!isConstructCall) {
                return JavaScriptValue.Invalid;
            }

            IntPtr objId;
            do {
                objId = new IntPtr(new Random().Next());
            } while(instances.ContainsKey(objId));
            var jsObj = JavaScriptValue.CreateExternalObject(objId, null);
            var actualObj = new JsXmlHttpRequest(jsObj);
            instances.Add(objId, actualObj);

            // Properties
            jsObj.SetProperty(JavaScriptPropertyId.FromString("readyState"), JavaScriptValue.FromInt32((int)ReadyStates.Unsent), true);
            jsObj.SetProperty(JavaScriptPropertyId.FromString("response"), JavaScriptValue.FromString(""), true);
            jsObj.SetProperty(JavaScriptPropertyId.FromString("responseText"), JavaScriptValue.FromString(""), true);
            jsObj.SetProperty(JavaScriptPropertyId.FromString("responseType"), JavaScriptValue.FromString(""), true);
            jsObj.SetProperty(JavaScriptPropertyId.FromString("responseUrl"), JavaScriptValue.FromString(""), true);
            //jsObj.SetProperty(JavaScriptPropertyId.FromString("responseXML"), JavaScriptValue.Null, true);
            jsObj.SetProperty(JavaScriptPropertyId.FromString("status"), JavaScriptValue.FromInt32(0), true);
            jsObj.SetProperty(JavaScriptPropertyId.FromString("statusText"), JavaScriptValue.FromString(""), true);
            //jsObj.SetProperty(JavaScriptPropertyId.FromString("timeout"), JavaScriptValue.FromInt32(0), true);
            //jsObj.SetProperty(JavaScriptPropertyId.FromString("upload"), JavaScriptValue.Null, true);
            //jsObj.SetProperty(JavaScriptPropertyId.FromString("withCredentials"), JavaScriptValue.FromBoolean(false), true);

            // Event properties 
            //jsObj.SetProperty(JavaScriptPropertyId.FromString("onabort"), JavaScriptValue.Null, true);
            jsObj.SetProperty(JavaScriptPropertyId.FromString("onerror"), JavaScriptValue.Null, true);
            jsObj.SetProperty(JavaScriptPropertyId.FromString("onload"), JavaScriptValue.Null, true);
            //jsObj.SetProperty(JavaScriptPropertyId.FromString("onloadstart"), JavaScriptValue.Null, true);
            //jsObj.SetProperty(JavaScriptPropertyId.FromString("onprogress"), JavaScriptValue.Null, true);
            //jsObj.SetProperty(JavaScriptPropertyId.FromString("ontimeout"), JavaScriptValue.Null, true);
            //jsObj.SetProperty(JavaScriptPropertyId.FromString("onloadend"), JavaScriptValue.Null, true);
            jsObj.SetProperty(JavaScriptPropertyId.FromString("onreadystatechange"), JavaScriptValue.Null, true);
            //jsObj.SetProperty(JavaScriptPropertyId.FromString("ontimeout"), JavaScriptValue.Null, true);

            // Methods
            //jsObj.SetProperty(JavaScriptPropertyId.FromString("abort"), JavaScriptValue.CreateFunction(functions["abort"]), true);
            //jsObj.SetProperty(JavaScriptPropertyId.FromString("getAllResponseHeaders"), JavaScriptValue.CreateFunction(functions["getAllResponseHeaders"]), true);
            jsObj.SetProperty(JavaScriptPropertyId.FromString("open"), JavaScriptValue.CreateFunction(functions["open"]), true);
            //jsObj.SetProperty(JavaScriptPropertyId.FromString("overrideMimeType"), JavaScriptValue.CreateFunction(functions["overrideMimeType"]), true);
            jsObj.SetProperty(JavaScriptPropertyId.FromString("send"), JavaScriptValue.CreateFunction(functions["send"]), true);
            jsObj.SetProperty(JavaScriptPropertyId.FromString("setRequestHeader"), JavaScriptValue.CreateFunction(functions["setRequestHeader"]), true);

            return jsObj;
        }



        private static JavaScriptValue abort(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            return JavaScriptValue.Invalid;
        }

        private static JavaScriptValue getAllResponseHeaders(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            return JavaScriptValue.Invalid;
        }

        private static JavaScriptValue open(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount < 3) {
                return JavaScriptValue.Invalid;
            }

            string method = arguments[1].ToString();
            string url = arguments[2].ToString();

            bool async = true;
            if(argumentCount > 3) {
                async = arguments[3].ToBoolean();
                if(async == false) {
                    return JavaScriptValue.Invalid;
                }
            }

            instances[arguments[0].ExternalData].Open(method, url);

            return JavaScriptValue.Invalid;
        }

        private static JavaScriptValue overrideMimeType(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            // TODO: ignored for now
            return JavaScriptValue.Invalid;
        }

        private static JavaScriptValue send(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            string body = null;
            if(argumentCount > 1 && arguments[1].ValueType == JavaScriptValueType.String) {
                body = arguments[1].ToString();
            }

            instances[arguments[0].ExternalData].Send(body);

            return JavaScriptValue.Undefined;
        }

        private static JavaScriptValue setRequestHeader(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            if(argumentCount != 3) {
                return JavaScriptValue.Invalid;
            }

            string header = arguments[1].ToString();
            string value = arguments[2].ToString();
            instances[arguments[0].ExternalData].SetRequestHeader(header, value);

            return JavaScriptValue.Undefined;
        }

        // Be sure to keep a copy of the function delegate around so it isn't garbage collected
        private static Dictionary<string, JavaScriptNativeFunction> functions = new Dictionary<string, JavaScriptNativeFunction>() {
            {"abort", abort},
            {"open", open},
            {"send", send},
            {"getAllResponseHeaders", getAllResponseHeaders},
            {"overrideMimeType", overrideMimeType},
            {"setRequestHeader", setRequestHeader},
        };
    }
}
