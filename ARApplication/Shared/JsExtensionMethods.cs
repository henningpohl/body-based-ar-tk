using ChakraHost.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Urho;

namespace BodyAR {
    static class JsExtensionMethods {
        public static int? Length(this JavaScriptValue v) {
            switch(v.ValueType) {
                case JavaScriptValueType.Array:
                case JavaScriptValueType.ArrayBuffer:
                case JavaScriptValueType.TypedArray:
                    return v.GetProperty(JavaScriptPropertyId.FromString("length")).ConvertToNumber().ToInt32();
                default:
                    return null;
            }
        }

        public static bool Has(this JavaScriptValue v, int i) {
            return v.HasIndexedProperty(JavaScriptValue.FromInt32(i));
        }

        public static bool Has(this JavaScriptValue v, string name) {
            return v.HasProperty(JavaScriptPropertyId.FromString(name));
        }

        public static JavaScriptValue Get(this JavaScriptValue v, int i) {
            return v.GetIndexedProperty(JavaScriptValue.FromInt32(i));
        }

        public static JavaScriptValue Get(this JavaScriptValue v, string name) {
            return v.GetProperty(JavaScriptPropertyId.FromString(name));
        }

        public static bool IsUndefined(this JavaScriptValue v) {
            return v.ValueType == JavaScriptValueType.Undefined;
        }

        public static JavaScriptValue ToJavaScriptValue<T>(this T x) where T : struct {
            var jsObj = JavaScriptValue.CreateObject();

            foreach(var field in x.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)) {
                var name = JavaScriptPropertyId.FromString(field.Name.ToLower());
                var value = field.GetValue(x);
                switch(value) {
                    case uint i:
                        jsObj.SetProperty(name, JavaScriptValue.FromInt32((int)i), true);
                        break;
                    case int i:
                        jsObj.SetProperty(name, JavaScriptValue.FromInt32(i), true);
                        break;
                    case float f:
                        jsObj.SetProperty(name, JavaScriptValue.FromDouble(f), true);
                        break;
                    case double d:
                        jsObj.SetProperty(name, JavaScriptValue.FromDouble(d), true);
                        break;
                    case string s:
                        jsObj.SetProperty(name, JavaScriptValue.FromString(s), true);
                        break;
                    case bool b:
                        jsObj.SetProperty(name, JavaScriptValue.FromBoolean(b), true);
                        break;
                }
            }
            return jsObj;
        }

        public static Vector3 ToVector3(this JavaScriptValue v) {
            switch(v.ValueType) {
                case JavaScriptValueType.Array:
                    var length = v.Length();
                    if(!length.HasValue || length.Value != 3) {
                        return new Vector3(0, 0, 0);
                    }
                    return new Vector3(
                        (float)v.Get(0).ToDouble(),
                        (float)v.Get(1).ToDouble(),
                        (float)v.Get(2).ToDouble()
                    );
                case JavaScriptValueType.Object:
                    if(v.Has("x") && v.Has("y") && v.Has("z")) {
                        return new Vector3(
                            (float)v.Get("x").ConvertToNumber().ToDouble(),
                            (float)v.Get("y").ConvertToNumber().ToDouble(),
                            (float)v.Get("z").ConvertToNumber().ToDouble()
                        );
                    } else {
                        return new Vector3(0, 0, 0);
                    }
                default:
                    return new Vector3(0, 0, 0);
            }
        }

        public static Color ToColor(this JavaScriptValue v) {
            switch(v.ValueType) {
                case JavaScriptValueType.String:
                    var s = v.ToString(); 
                    // TODO: do this
                    return Color.White;
                case JavaScriptValueType.Object:
                    if(v.Has("r") && v.Has("g") && v.Has("b")) {
                        var a = v.Has("a") ? v.Get("a").ConvertToNumber().ToInt32() : 255;
                        return Color.FromByteFormat(
                            (byte)v.Get("r").ConvertToNumber().ToInt32(),
                            (byte)v.Get("g").ConvertToNumber().ToInt32(),
                            (byte)v.Get("b").ConvertToNumber().ToInt32(),
                            (byte)a);
                    } else {
                        return Color.White;
                    }
                case JavaScriptValueType.Array:
                    var length = v.Length();
                    if(length == 4) {
                        return Color.FromByteFormat(
                            (byte)v.Get(0).ConvertToNumber().ToInt32(),
                            (byte)v.Get(1).ConvertToNumber().ToInt32(),
                            (byte)v.Get(2).ConvertToNumber().ToInt32(),
                            (byte)v.Get(3).ConvertToNumber().ToInt32());
                    } else if(length == 3) {
                        return Color.FromByteFormat(
                            (byte)v.Get(0).ConvertToNumber().ToInt32(),
                            (byte)v.Get(1).ConvertToNumber().ToInt32(),
                            (byte)v.Get(2).ConvertToNumber().ToInt32(),
                            0);
                    } else {
                        return Color.White;
                    }
                default:
                    return Color.White;
            }
        }
    }
}
