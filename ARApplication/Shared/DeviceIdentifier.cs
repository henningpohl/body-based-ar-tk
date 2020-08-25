using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.System.Profile;

namespace BodyAR {
    // https://docs.microsoft.com/en-us/previous-versions/windows/apps/jj553431(v=win.10)
    class DeviceIdentifier {

        enum Component {
            Processor = (1 << 8),
            Memory = (2 << 8),
            DiskDevice = (3 << 8),
            NetworkAdapter = (4 << 8),
            AudioAdapter = (5 << 8),
            DockingStation = (6 << 8),
            MobileBroadband = (7 << 8),
            Bluetooth = (8 << 8),
            SystemBIOS = (9 << 8)
        }
        
        public static string GetDeviceId() {
            var nonce = CryptographicBuffer.ConvertStringToBinary("0.1", BinaryStringEncoding.Utf8);
            var ashwid = HardwareIdentification.GetPackageSpecificToken(nonce);

            var accum = new Dictionary<Component, int>();
            var idstream = ashwid.Id.AsStream();
            for(int i = 0; i < idstream.Length; i += 4) {
                var componentID = (Component)((idstream.ReadByte() << 8) | idstream.ReadByte());
                var componentValue = (idstream.ReadByte() << 8) | idstream.ReadByte();

                switch(componentID) {
                    case Component.Processor:
                    case Component.SystemBIOS:
                    case Component.Memory:
                        accum[componentID] = accum.GetValueOrDefault(componentID) + componentValue;
                       break;
                    default:
                        break;
                }
            }

            var pid = accum.GetValueOrDefault(Component.Processor).ToString("x");
            var sid = accum.GetValueOrDefault(Component.SystemBIOS).ToString("x");
            var mid = accum.GetValueOrDefault(Component.Memory).ToString("x");
            return $"{pid}-{sid}-{mid}";
        }
    }
}
