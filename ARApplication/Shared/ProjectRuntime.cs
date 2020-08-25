using SocketIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace BodyAR {
    public class ProjectRuntime {
        internal Client client;
        private JsRuntime runtime;
        private JsInterop interop;
        private Queue<string> cmdQueue = new Queue<string>();
        private Queue<Action> actionQueue = new Queue<Action>();

        private enum AppStatus {
            Initializing,
            WaitingForProject,
            LoadingProject,
            ExecutingProject
        }
        private AppStatus status = AppStatus.Initializing;

        public class StatusEventArgs : EventArgs {
            public string Status;
        }
        public event EventHandler<StatusEventArgs> StatusTextChanged;

        private string deviceID;
        private int projectID = -1;

        private int frameNumber;
        private DateTime startTime;

        public static ProjectRuntime Inst {
            get; private set;
        }

        public ProjectRuntime() {
            Inst = this;
        }

        public void Connect(string appName = "") {
            deviceID = DeviceIdentifier.GetDeviceId();

            client = new Client();
            Uri wsUri = new UriBuilder("ws", Configuration.PROJECT_SERVER, Configuration.PROJECT_SERVER_PORT, "/socket.io/", "?EIO=4&transport=websocket").Uri;
            client.Connect(wsUri);

            client.On("run", (m) => {
                var mdata = m.GetArray().GetObjectAt(0);
                var did = mdata.GetNamedString("did");
                var pid = (int)mdata.GetNamedNumber("pid");
                if(deviceID.Equals(did)) {
                    LoadProject(pid);
                }
            });

            client.On("updatedValue", (m) => {
                var mdata = m.GetArray().GetObjectAt(0);
                var pid = (int)mdata.GetNamedNumber("pid");
                if(pid != projectID) {
                    return;
                }

                var nid = mdata.GetNamedString("nid");
                var ntype = mdata.GetNamedString("ntype");
                var nvalue = mdata.GetNamedValue("nvalue").ToString();

                var cmdString = $"app.nodes['{nid}'].setValue('{ntype}', {nvalue});";
                DispatchRuntimeCode(cmdString);
            });

            JsonObject deviceInfo = new JsonObject();
            deviceInfo.SetNamedValue("name", JsonValue.CreateStringValue(appName));
            deviceInfo.SetNamedValue("status", JsonValue.CreateStringValue("ready"));
            deviceInfo.SetNamedValue("did", JsonValue.CreateStringValue(deviceID));
            client.Emit("addDevice", deviceInfo);

            status = AppStatus.WaitingForProject;
        }

        public async Task SetCamera(ISceneCamera camera) {
            await SceneCameraManager.Inst.Init(camera);
        }

        private void LoadProject(int pid) {
            if(status != AppStatus.WaitingForProject) {
                return; // update later to handle the other cases
            }

            StatusTextChanged?.Invoke(this, new StatusEventArgs() { Status = "Loading project..." });
            SceneCameraManager.Inst.Start();

            projectID = pid;
            Uri projectUri = new UriBuilder("http", Configuration.PROJECT_SERVER, Configuration.PROJECT_SERVER_PORT, $"pack/{projectID}").Uri;
            runtime.Execute(projectUri).ContinueWith((t) => {
                if(t.Status == TaskStatus.RanToCompletion) {
                    status = AppStatus.ExecutingProject;
                    StatusTextChanged?.Invoke(this, new StatusEventArgs() { Status = "" });
                }
            });
        }

        public void DispatchRuntimeCode(String cmd) {
            cmdQueue.Enqueue(cmd);
        }

        public void DispatchRuntimeCode(Action action) {
            actionQueue.Enqueue(action);
        }

        public void Start() {
            interop = new JsInterop();

            StatusTextChanged?.Invoke(this, new StatusEventArgs() { Status = "Waiting for editor to start project" });

            runtime = new JsRuntime();
            runtime.Init();
            runtime.RegisterObject("artk", interop);

            frameNumber = 0;
            startTime = DateTime.Now;

            /*
            runtime.Execute(@"var httpTest = new XMLHttpRequest();");
            runtime.Execute(@"httpTest.onload = function() {console.log(httpTest.statusText); };");
            runtime.Execute(@"httpTest.open('GET', 'http://www.google.com'); ");
            runtime.Execute(@"httpTest.send();");
            */
            //runtime.Execute(@"artk.updateBarplot(""test"", [0, 1, 0], [{value: 0.5, label: ""A""}, {value: 1.5, label: ""B"", color: {r: 255, g: 100, b: 100}}]);");
            //runtime.Execute(@"artk.updateLabel(""x"", [1, 1, 0], {r:255, g:0, b:0}, ""test"");");
            //runtime.Execute(@"artk.updatePath(""p0"", [[0, 0, 0], [2, 0, 0], [2, 0, 2], [2, 2, 2]], {r:200, g:180, b:50});");
        }

        public void Update(float timePassed) {
            if(status == AppStatus.ExecutingProject) {
                while(cmdQueue.Count > 0) {
                    runtime.Execute(cmdQueue.Dequeue());
                }
                while(actionQueue.Count > 0) {
                    runtime.Execute(actionQueue.Dequeue());
                }

                runtime.Execute($"artk.projectID = {projectID};");
                runtime.Execute($"artk.frame = {frameNumber};");
                runtime.Execute($"artk.localTime = Date.now();");
                runtime.Execute($"artk.appTime = {(DateTime.Now - startTime).TotalMilliseconds};");
                runtime.Execute($"artk.frameTime = {1000 * timePassed};");
                frameNumber++;

                runtime.Execute(@"app.update();");
            }
        }
    }
}
