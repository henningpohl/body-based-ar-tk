using BodyAR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Perception.Spatial;

namespace HoloLens {
    // https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/process-media-frames-with-mediaframereader
    class HoloLensCamera : ISceneCamera {
        public bool ShouldRender => false;
        
        public int Width {
            get; private set;
        }

        public int Height {
            get; private set;
        }

        public Matrix4 ProjectionMatrix {
            get; private set;
        }

        public event CameraFrameReadyEventHandler FrameReady;

        private MediaCapture mediaCapture;
        private MediaFrameReader mediaFrameReader;

        private SpatialLocator locator;
        private SpatialStationaryFrameOfReference originalFrameOfReference;

        public HoloLensCamera() {
            locator = SpatialLocator.GetDefault();
            originalFrameOfReference = locator.CreateStationaryFrameOfReferenceAtCurrentLocation();
        }

        public async Task<bool> InitAsync() {
            var sourceGroup = await GetMediaSourceGroup("MN34150"); // name of the HoloLens camera
            if(sourceGroup == null) {
                System.Diagnostics.Debug.WriteLine("HoloLens camera not found");
                return false;
            }

            await InitializeMediaCapture(sourceGroup);
            var source = mediaCapture.FrameSources
                .Where(fs => fs.Value.Info.MediaStreamType == MediaStreamType.VideoRecord)
                .Select(x => x.Value)
                .FirstOrDefault();
            if(source == null) {
                System.Diagnostics.Debug.WriteLine("No valid media source found");
                return false;
            }

            mediaFrameReader = await mediaCapture.CreateFrameReaderAsync(source);
            mediaFrameReader.AcquisitionMode = MediaFrameReaderAcquisitionMode.Buffered;
            mediaFrameReader.FrameArrived += MediaFrameReader_FrameArrived;

            return true;
        }

        public async Task StartCameraAsync() {
            await mediaFrameReader?.StartAsync();
        }

        // https://github.com/MarekKowalski/HoloFace/blob/master/HoloFace/Assets/HololensCameraUWP.cs
        private void MediaFrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args) {
            using(var frame = sender.TryAcquireLatestFrame()) {

                // https://docs.microsoft.com/en-us/windows/mixed-reality/locatable-camera
                var coordinateSystem = frame?.CoordinateSystem;
                var cameraIntrinsics = frame?.VideoMediaFrame?.CameraIntrinsics;

                var ht = coordinateSystem.TryGetTransformTo(originalFrameOfReference.CoordinateSystem);
                
                Matrix4 webcamToWorldMatrix = new Matrix4(
                    ht?.M11 ?? 1, ht?.M21 ?? 0, ht?.M31 ?? 0, ht?.Translation.X ?? 0,
                    ht?.M12 ?? 0, ht?.M22 ?? 1, ht?.M32 ?? 0, ht?.Translation.Y ?? 0,
                    -ht?.M13 ?? 0, -ht?.M23 ?? 0, -ht?.M33 ?? 1, -ht?.Translation.Z ?? 0,
                    0, 0, 0, 1);

                using(var bitmap = frame?.VideoMediaFrame?.SoftwareBitmap) {
                    if(bitmap == null) {
                        return;
                    }

                    Width = bitmap.PixelWidth;
                    Height = bitmap.PixelHeight;

                    var projectionMatrix = new Matrix4();
                    projectionMatrix.M11 = 2 * cameraIntrinsics.FocalLength.X / Width;
                    projectionMatrix.M22 = 2 * cameraIntrinsics.FocalLength.Y / Height;
                    projectionMatrix.M13 = -2 * (cameraIntrinsics.PrincipalPoint.X - Width / 2) / Width;
                    projectionMatrix.M23 = 2 * (cameraIntrinsics.PrincipalPoint.Y - Height / 2) / Height;
                    projectionMatrix.M33 = -1;
                    projectionMatrix.M44 = -1;
                    ProjectionMatrix = projectionMatrix;

                    var copy = SoftwareBitmap.Copy(bitmap);
                    FrameReady?.Invoke(new FrameData() {
                        bitmap = copy,
                        webcamToWorldMatrix = webcamToWorldMatrix,
                        projectionMatrix = projectionMatrix
                    });
                }
            }
        }

        public async Task StopCameraAsync() {
            await mediaFrameReader?.StopAsync();
        }

        private async Task InitializeMediaCapture(MediaFrameSourceGroup sourceGroup) {
            mediaCapture = new MediaCapture();

            await mediaCapture.InitializeAsync(new MediaCaptureInitializationSettings() {
                SourceGroup = sourceGroup,
                SharingMode = MediaCaptureSharingMode.SharedReadOnly,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                StreamingCaptureMode = StreamingCaptureMode.Video
            });

            mediaCapture.Failed += (s, e) => {
                System.Diagnostics.Debug.WriteLine(e.Message);
            };

            mediaCapture.RecordLimitationExceeded += async s => {
                System.Diagnostics.Debug.WriteLine("Record limitation exceeded");
                await StopCameraAsync();
            };

            mediaCapture.CameraStreamStateChanged += (s, e) => {
                System.Diagnostics.Debug.WriteLine("Camera stream state changed to: " + s.CameraStreamState);
            };
        }

        private async Task<MediaFrameSourceGroup> GetMediaSourceGroup(string name) {
            var groups = await MediaFrameSourceGroup.FindAllAsync();
            foreach(var group in groups) {
                if(group.DisplayName == name) {
                    return group;
                }
            }
            return null;
        }
    }
}
