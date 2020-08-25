using BodyAR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Audio;
using Urho.SharpReality;

namespace HoloLens {
    class HoloLensApplication : StereoApplication {

        private ProjectRuntime runtime;
        private HoloLensCamera holoCamera;
        private Gui gui;

        public HoloLensApplication(ApplicationOptions opts) : base(opts) {
            runtime = new ProjectRuntime();
            runtime.StatusTextChanged += (s, e) => {
                gui.Status = e.Status;
            };
            holoCamera = new HoloLensCamera();
        }

        protected override async void Start() {
            base.Start();

            // Enable input
            EnableGestureManipulation = false;
            EnableGestureTapped = false;

            RegisterCortanaCommands(new Dictionary<string, Action>() {
                {"Snap", () => { System.Diagnostics.Debug.WriteLine("Snap"); } }
            });

            var soundListener = LeftCamera.Node.CreateComponent<SoundListener>();
            Audio.Listener = soundListener;
            LeftCamera.Node.CreateComponent<UserTracker>();

            Scene.AddComponent(FloatManager.Inst);
            FloatManager.Inst.Init();
            gui = new Gui();
            Scene.CreateChild("Sounds").CreateComponent<SoundManager>();
            Scene.CreateChild("Models").CreateComponent<ModelManager>();
            Scene.CreateChild("Images").CreateComponent<ImageManager>();

            if(Configuration.DEBUG_MODE) {
                Scene.CreateComponent<DebugRenderer>();
            }

            runtime.Start();
            runtime.SetCamera(holoCamera);
            runtime.Connect(appName: "HoloLens App");

            // Scene has a lot of pre-configured components, such as Cameras (eyes), Lights, etc.
            DirectionalLight.Brightness = 1f;
            DirectionalLight.Node.SetDirection(new Vector3(-1, 0, 0.5f));

            Engine.PostRenderUpdate += _ => {
                if(Configuration.DEBUG_MODE) {
                    Renderer.DrawDebugGeometry(false);
                }
            };
        }

        protected override void OnUpdate(float timeStep) {
            base.OnUpdate(timeStep);
            runtime.Update(timeStep);

            
        }

        // For HL optical stabilization (optional)
        //public override Vector3 FocusWorldPoint => earthNode.WorldPosition;

        //Handle input:
        public override void OnGestureManipulationStarted() { }
        public override void OnGestureManipulationUpdated(Vector3 relativeHandPosition) { }

        public override void OnGestureTapped() { }
        public override void OnGestureDoubleTapped() { }
    }
}
