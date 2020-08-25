using ChakraHost.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace BodyAR {
    class JsRuntime : IDisposable {
        public class JsExport : System.Attribute {
            public string JavaScriptMethodName = null;
        }

        private JavaScriptRuntime runtime;
        private JavaScriptContext context;

        private List<JavaScriptNativeFunction> registeredFunctions = new List<JavaScriptNativeFunction>();

        public JsRuntime() {
            Reset();
        }

        public void Reset() {
            Dispose();
            runtime = JavaScriptRuntime.Create();
            context = runtime.CreateContext();
        }

        public void Init() {
            JavaScriptNativeFunction printFunc = PrintFunc;
            registeredFunctions.Add(printFunc);
            JavaScriptNativeFunction httpFunc = JsXmlHttpRequest.JsConstructor;
            registeredFunctions.Add(httpFunc);

            using(new JavaScriptContext.Scope(context)) {
                var printFuncName = JavaScriptPropertyId.FromString("print");
                var printFuncObj = JavaScriptValue.CreateFunction(printFunc);
                JavaScriptValue.GlobalObject.SetProperty(printFuncName, printFuncObj, true);

                var logObj = JavaScriptValue.CreateObject();
                var logFuncName = JavaScriptPropertyId.FromString("log");
                logObj.SetProperty(logFuncName, printFuncObj, true);

                var consoleName = JavaScriptPropertyId.FromString("console");
                JavaScriptValue.GlobalObject.SetProperty(consoleName, logObj, true);

                var xmlHttpRequestName = JavaScriptPropertyId.FromString("XMLHttpRequest");
                var httpFuncObj = JavaScriptValue.CreateFunction(httpFunc);
                JavaScriptValue.GlobalObject.SetProperty(xmlHttpRequestName, httpFuncObj, true);
            }
        }

        public void RegisterGlobalFunction(string name, JavaScriptNativeFunction func) {
            registeredFunctions.Add(func);
            using(new JavaScriptContext.Scope(context)) {
                var funcName = JavaScriptPropertyId.FromString(name);
                var funcObj = JavaScriptValue.CreateFunction(func);
                JavaScriptValue.GlobalObject.SetProperty(funcName, funcObj, true);
            }
        }

        public void RegisterObject(string name, object obj) {
            var type = obj.GetType();
            using(new JavaScriptContext.Scope(context)) {
                var jsObj = JavaScriptValue.CreateObject();
                
                foreach(var m in type.GetMethods()) {
                    var attrib = m.GetCustomAttribute<JsExport>();
                    if(attrib == null) {
                        continue;
                    }

                    var funcName = attrib.JavaScriptMethodName ?? m.Name;
                    var funcNameProp = JavaScriptPropertyId.FromString(funcName);

                    var funcDelegate = (JavaScriptNativeFunction)Delegate.CreateDelegate(typeof(JavaScriptNativeFunction), obj, m);
                    registeredFunctions.Add(funcDelegate);
                    var funcObj = JavaScriptValue.CreateFunction(funcDelegate);

                    jsObj.SetProperty(funcNameProp, funcObj, true);
                }

                var nameProp = JavaScriptPropertyId.FromString(name);
                JavaScriptValue.GlobalObject.SetProperty(nameProp, jsObj, true);
            }
        }

        public void Execute(string script) {
            using(new JavaScriptContext.Scope(context)) {
                try {
                    var result = JavaScriptContext.RunScript(script);
                } catch(JavaScriptScriptException e) {
                    var message = e.Error.Get("message").ConvertToString().ToString();
                    if(e.Error.Has("line") && e.Error.Has("column")) {
                        var line = e.Error.Get("line").ToInt32();
                        var col = e.Error.Get("column").ToInt32();
                        System.Diagnostics.Debug.WriteLine($"JavaScriptError on {line}:{col} => {message}");
                    } else {
                        System.Diagnostics.Debug.WriteLine($"JavaScriptError => {message}");
                    }
                } catch(JavaScriptUsageException e) {
                    System.Diagnostics.Debug.WriteLine($"Usage exception during script execution: {e.Message}");
                    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                } catch(Exception e) {
                    System.Diagnostics.Debug.WriteLine($"Exception during script execution: {e.Message}");
                }
            }
        }

        public void Execute(JavaScriptValue function) {
            if(function.ValueType != JavaScriptValueType.Function) {
                return;
            }

            using(new JavaScriptContext.Scope(context)) {   
                function.CallFunction(function);
            }
        }

        public void Execute(Action action) {
            using(new JavaScriptContext.Scope(context)) {
                action.Invoke();
            }
        }

        public async Task Execute(Uri uri) {
            var http = new HttpClient();
            var response = await http.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadAsStringAsync();
            Execute(responseData);
        }

        public void Dispose() {
            if(context.IsValid) {
                context.Release();
            }
            if(runtime.IsValid) {
                runtime.Dispose();
            }
        }

        private JavaScriptValue PrintFunc(JavaScriptValue callee, bool isConstructCall, JavaScriptValue[] arguments, ushort argumentCount, IntPtr callbackData) {
            for(int i = 1; i < argumentCount; ++i) {
                if(i > 1) {
                    System.Diagnostics.Debug.Write(" ");
                }
                System.Diagnostics.Debug.Write(arguments[i].ConvertToString().ToString());
            }
            System.Diagnostics.Debug.WriteLine("");
            return JavaScriptValue.Invalid;
        }
    }
}
