using BodyAR;
using LocalTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;


static class AppAdapter {
    public static Scene GetScene(this Application app) {
        return ((LocalApplication)app).Scene;
    }

    public static Vector3 GetHeadPosition(this Application app) {
        return ((LocalApplication)app).HeadPosition;
    }
}

