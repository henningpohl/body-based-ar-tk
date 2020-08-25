using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Gui;

namespace BodyAR {
    public sealed class FloatManager : Component {

        private static FloatManager _inst = null;
        public static FloatManager Inst {
            get {
                if(_inst == null) {
                    _inst = new FloatManager();
                }
                return _inst;
            }
        }

        private FloatManager() { }

        public void Init() {

        }
    }
}
