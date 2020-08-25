using ChakraHost.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;

namespace BodyAR {
    class UserTracker : Component {

        private List<JavaScriptValue> callbacks = new List<JavaScriptValue>();
        private const float UPDATE_INTERVAL = 1.0f / 30.0f;
        private float time = 0.0f;

        public static UserTracker Inst {
            get; private set;
        }

        public UserTracker() {
            Inst = this;
            ReceiveSceneUpdates = true;
        }

        public void AddCallback(JavaScriptValue callback) {
            if(callback.ValueType != JavaScriptValueType.Function) {
                throw new ArgumentException();
            }
            callback.AddRef();
            callbacks.Add(callback);
        }

        public override void OnAttachedToNode(Node node) {
            base.OnAttachedToNode(node);
        }

        protected override void OnUpdate(float timeStep) {
            base.OnUpdate(timeStep);

            time += timeStep;
            if(time < UPDATE_INTERVAL) {
                return;
            } else {
                time -= UPDATE_INTERVAL;
            }

            if(callbacks.Count == 0) {
                return;
            }

            ProjectRuntime.Inst.DispatchRuntimeCode(() => {
                var position = JavaScriptContext.RunScript($"new Position({Node.WorldPosition.X}, {Node.WorldPosition.Y}, {Node.WorldPosition.Z});");
                var forwardVec = Node.WorldRotation * Vector3.Forward;
                var forward = JavaScriptContext.RunScript($"new Position({forwardVec.X}, {forwardVec.Y}, {forwardVec.Z});");
                var upVec = Node.WorldRotation * Vector3.Up;
                var up = JavaScriptContext.RunScript($"new Position({upVec.X}, {upVec.Y}, {upVec.Z});");
                var rightVec = Node.WorldRotation * Vector3.Right;
                var right = JavaScriptContext.RunScript($"new Position({rightVec.X}, {rightVec.Y}, {rightVec.Z});");
                foreach(var callback in callbacks) {
                    callback.CallFunction(callback, position, forward, up, right);
                }
            });
            
        }
    }
}
