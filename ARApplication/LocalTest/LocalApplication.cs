using BodyAR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Audio;

namespace LocalTest {
    class LocalApplication : Application {
        private float Yaw = 90.0f;
        private float Pitch = 45.0f;
        private float mouseSensitivity = 0.5f;

        private MockSceneCamera mockCamera;
        private ProjectRuntime runtime;

        public Scene Scene { get; private set; }
        private Octree octree;
        private Node cameraNode;
        private Camera camera;
        private Gui gui;

        private const float HOLOLENS_FOV = 45.0f;
        private const float HOLOLENS_NEAR_PLANE = 0.1f;
        private const float HOLOLENS_FAR_PLANE = 1000.0f;
        private const float HOLOLENS_ASPECT_RATIO = 1.0f;
        private const float HOLOLENS_SKEW = 0.0f;
        private Vector2 HOLOLENS_PROJECTION_OFFSET = new Vector2(0.0f, 0.0f);

        public Vector3 HeadPosition => cameraNode.WorldPosition;
        public Matrix3x4 CameraToWorldMatrix => camera.View.Inverse();

        public LocalApplication(ApplicationOptions options = null) : base(options) {
            runtime = new ProjectRuntime();
            runtime.StatusTextChanged += (s, e) => {
                gui.Status = e.Status;
            };
        }

        protected override void Start() {
            base.Start();

            Graphics.WindowTitle = "Body-AR Toolkit";
            var cache = ResourceCache;

            Scene = new Scene();
            octree = Scene.CreateComponent<Octree>();

            cameraNode = Scene.CreateChild();
            cameraNode.Position = new Vector3(-3, 3, 0);
            cameraNode.Rotation = new Quaternion(Pitch, Yaw, 0);
            camera = cameraNode.CreateComponent<Camera>();
            camera.Fov = HOLOLENS_FOV;
            camera.NearClip = HOLOLENS_NEAR_PLANE;
            camera.FarClip = HOLOLENS_FAR_PLANE;
            camera.ProjectionOffset = HOLOLENS_PROJECTION_OFFSET;
            camera.Skew = HOLOLENS_SKEW;
            //camera.AspectRatio = HOLOLENS_ASPECT_RATIO;
            var soundListener = cameraNode.CreateComponent<SoundListener>();
            Audio.Listener = soundListener;
            cameraNode.CreateComponent<UserTracker>();

            var zone = Scene.CreateChild().CreateComponent<Zone>();
            zone.AmbientColor = new Color(0.5f, 0.5f, 0.5f);

            gui = new Gui();
            Scene.AddComponent(FloatManager.Inst);
            Scene.CreateChild("Sounds").CreateComponent<SoundManager>();
            Scene.CreateChild("Models").CreateComponent<ModelManager>();
            Scene.CreateChild("Images").CreateComponent<ImageManager>();
            FloatManager.Inst.Init();

            if(Configuration.DEBUG_MODE) {
                Scene.CreateComponent<DebugRenderer>();
            }

            Renderer.SetViewport(0, new Viewport(Context, Scene, camera, null));

            runtime.Start();
            mockCamera = new MockSceneCamera();
            runtime.SetCamera(mockCamera);
            runtime.Connect(appName: "Local Testapp");

            Input.KeyDown += (e) => {
                switch(e.Key) {
                    case Key.Esc:
                        InvokeOnMain(() => {
                            //Exit(); // app hangs upon calling this O_o
                        });
                        break;
                }
            };

            Engine.PostRenderUpdate += _ => {
                if(Configuration.DEBUG_MODE) {
                    Renderer.DrawDebugGeometry(false);
                }
            };
        }

        protected override void OnUpdate(float timeStep) {
            base.OnUpdate(timeStep);
            runtime.Update(timeStep);

            if(Input.NumTouches > 0) { // urho in uwp mode handles mouse as touch
                var touchState = Input.GetTouch(0);
                Yaw += mouseSensitivity * touchState.Delta.X;
                Pitch += mouseSensitivity * touchState.Delta.Y;
                Pitch = MathHelper.Clamp(Pitch, -90, 90);

                cameraNode.Rotation = new Quaternion(Pitch, Yaw, 0);
            }

            float moveSpeed = 10.0f;
            if(Input.GetKeyDown(Key.W)) cameraNode.Translate(Vector3.UnitZ * moveSpeed * timeStep);
            if(Input.GetKeyDown(Key.S)) cameraNode.Translate(-Vector3.UnitZ * moveSpeed * timeStep);
            if(Input.GetKeyDown(Key.A)) cameraNode.Translate(-Vector3.UnitX * moveSpeed * timeStep);
            if(Input.GetKeyDown(Key.D)) cameraNode.Translate(Vector3.UnitX * moveSpeed * timeStep);            
        }
    }
}
