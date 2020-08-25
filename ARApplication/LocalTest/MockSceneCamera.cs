using BodyAR;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;

namespace LocalTest {
    // https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/play-audio-and-video-with-mediaplayer
    // https://stackoverflow.com/questions/49824091/how-to-get-softwarebitmap-from-mediaplayer-in-uwp
    class MockSceneCamera : ISceneCamera {
        public event CameraFrameReadyEventHandler FrameReady;

        private MediaPlayer mediaPlayer;
        private float dpi;
        private CoreWindow window;
        private Node cameraNode;
        private Camera camera;
        private SoftwareBitmap frameBuffer;

        public bool ShouldRender {
            get { return true; }
        }

        public int Width {
            get; private set;
        }
        public int Height {
            get; private set;
        }

        public Matrix4 ProjectionMatrix => camera.Projection;

        public MockSceneCamera() {
            window = CoreWindow.GetForCurrentThread();
            dpi = DisplayInformation.GetForCurrentView().LogicalDpi;
            cameraNode = Application.Current.GetScene().GetChildrenWithComponent<Camera>()?[0];
            camera = cameraNode?.GetComponent<Camera>();
        }

        public Task<bool> InitAsync() {
            mediaPlayer = new MediaPlayer();
            // https://www.youtube.com/watch?v=sh6XoWWuxno
            mediaPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/ExampleVideo01.mp4"));
            mediaPlayer.VideoFrameAvailable += VideoFrameAvailable;
            mediaPlayer.IsVideoFrameServerEnabled = true;
            mediaPlayer.IsMuted = true;

            return Task.FromResult(true);
        }

        private async void VideoFrameAvailable(MediaPlayer sender, object args) {
            CanvasDevice canvasDevice = CanvasDevice.GetSharedDevice();
            int width = (int)sender.PlaybackSession.NaturalVideoWidth;
            int height = (int)sender.PlaybackSession.NaturalVideoHeight;
            if(frameBuffer == null) {
                frameBuffer = new SoftwareBitmap(BitmapPixelFormat.Rgba8, width, height, BitmapAlphaMode.Premultiplied);
            }

            await window.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {
                SoftwareBitmap frame;

                using(var inputBitmap = CanvasBitmap.CreateFromSoftwareBitmap(canvasDevice, frameBuffer)) {
                    sender.CopyFrameToVideoSurface(inputBitmap);
                    frame = await SoftwareBitmap.CreateCopyFromSurfaceAsync(inputBitmap);
                }

                Width = frame.PixelWidth;
                Height = frame.PixelHeight;

                var cam = camera.View.Inverse();
                var webcamToWorld = new Matrix4(cam.m00, cam.m01, cam.m02, cam.m03,
                                                cam.m10, cam.m11, cam.m12, cam.m13,
                                                cam.m20, cam.m21, cam.m22, cam.m23,
                                                0, 0, 0, 1);

                FrameReady?.Invoke(new FrameData() {
                    bitmap = frame,
                    webcamToWorldMatrix = webcamToWorld,
                    projectionMatrix = camera.Projection
                });
            });
        }

        public async Task StartCameraAsync() {
            mediaPlayer.Play();
        }

        public async Task StopCameraAsync() {
            mediaPlayer.Pause();
        }
    }
}
