using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Windows.Graphics.Imaging;

namespace BodyAR {
    public class FrameData {
        public SoftwareBitmap bitmap;
        public Matrix4 webcamToWorldMatrix;
        public Matrix4 projectionMatrix;
    }

    public delegate void CameraFrameReadyEventHandler(FrameData frame);

    public interface ISceneCamera {

        Task<bool> InitAsync();
        Task StartCameraAsync();
        Task StopCameraAsync();

        event CameraFrameReadyEventHandler FrameReady;

        bool ShouldRender { get; }
        int Width { get; }
        int Height { get; }
        Matrix4 ProjectionMatrix { get; }
    }
}
