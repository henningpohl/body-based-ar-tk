using HoloLens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;

public static class AppAdapter {
    public static Scene GetScene(this Application app) {
        return ((HoloLensApplication)app).Scene;
    }

    public static Vector3 GetHeadPosition(this Application app) {
        return ((HoloLensApplication)app).HeadPosition;
    }
}

