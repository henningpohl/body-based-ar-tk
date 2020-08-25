using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Urho;
using Urho.Audio;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace BodyAR {
    class SoundManager : Component {
        public static SoundManager Inst { get; private set; }

        private SpeechSynthesizer speechSynthesizer;

        public SoundManager() {
            Inst = this;
            speechSynthesizer = new SpeechSynthesizer();

            var mark = from v in SpeechSynthesizer.AllVoices
                       where v.DisplayName.Contains("r")
                       select v;
            speechSynthesizer.Voice = mark.First();
        }

        public override void OnAttachedToNode(Node node) {
            base.OnAttachedToNode(node);
        }

        public async Task SayText(string text) {
            var stream = await speechSynthesizer.SynthesizeTextToStreamAsync(text);
            PlaySound(await SpeechStreamToSoundStream(stream));
        }

        public async Task SayText(string text, Vector3 position) {
            var stream = await speechSynthesizer.SynthesizeTextToStreamAsync(text);
            PlaySpatialSound(await SpeechStreamToSoundStream(stream), position);
        }

        // https://github.com/microsoft/MixedRealityCompanionKit/blob/master/LegacySpectatorView/Samples/SharedHolograms/Assets/HoloToolkit/Utilities/Scripts/TextToSpeech.cs
        private static int BytesToInt(byte[] bytes, int offset = 0) {
            int value = 0;
            for(int i = 0; i < 4; i++) {
                value |= ((int)bytes[offset + i]) << (i * 8);
            }
            return value;
        }

        // https://github.com/microsoft/MixedRealityCompanionKit/blob/master/LegacySpectatorView/Samples/SharedHolograms/Assets/HoloToolkit/Utilities/Scripts/TextToSpeech.cs
        private async Task<BufferedSoundStream> SpeechStreamToSoundStream(SpeechSynthesisStream inStream) {
            var outStream = new BufferedSoundStream();

            uint size = (uint)inStream.Size;
            byte[] wavAudio = new byte[size];

            using(var inputStream = inStream.GetInputStreamAt(0)) {
                inStream.Dispose();
                using(var reader = new DataReader(inputStream)) {
                    await reader.LoadAsync(size);
                    reader.ReadBytes(wavAudio);
                }
            }

            int channelCount = wavAudio[22];
            int frequency = BytesToInt(wavAudio, 24);
            outStream.SetFormat((uint)frequency, true, channelCount == 2);

            // Get past all the other sub chunks to get to the data subchunk:
            int pos = 12; // First subchunk ID from 12 to 16
            // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
            while(!(wavAudio[pos] == 100 && wavAudio[pos + 1] == 97 && wavAudio[pos + 2] == 116 && wavAudio[pos + 3] == 97)) {
                pos += 4;
                int chunkSize = wavAudio[pos] + wavAudio[pos + 1] * 256 + wavAudio[pos + 2] * 65536 + wavAudio[pos + 3] * 16777216;
                pos += 4 + chunkSize;
            }
            pos += 8;

            // Pos is now positioned to start of actual sound data.
            int sampleCount = (wavAudio.Length - pos) / 2;  // 2 bytes per sample (16 bit sound mono)
            if(channelCount == 2) { sampleCount /= 2; }  // 4 bytes per sample (16 bit stereo)

            outStream.AddData(wavAudio, pos);

            return outStream;
        }

        public void PlaySound(SoundStream stream) {
            var soundSource = CreateSoundNode().CreateComponent<SoundSource>();
            soundSource.AutoRemoveMode = AutoRemoveMode.Node;
            soundSource.Play(stream);
        }

        public void PlaySound(Sound sound) {
            var soundSource = CreateSoundNode().CreateComponent<SoundSource>();
            soundSource.AutoRemoveMode = AutoRemoveMode.Node;
            soundSource.Play(sound);
        }

        public void PlaySound(String soundID) {
            PlaySound(Application.ResourceCache.GetSound(soundID));
        }

        public void PlaySpatialSound(Sound sound, Vector3 position) {
            var soundSource = CreateSoundNode(position).CreateComponent<SoundSource3D>();
            soundSource.AutoRemoveMode = AutoRemoveMode.Node;
            soundSource.Play(sound);
        }

        public void PlaySpatialSound(SoundStream stream, Vector3 position) {
            var soundSource = CreateSoundNode(position).CreateComponent<SoundSource3D>();
            soundSource.AutoRemoveMode = AutoRemoveMode.Node;
            soundSource.Play(stream);
        }

        public void PlaySpatialSound(String soundID, Vector3 position) {
            PlaySpatialSound(Application.ResourceCache.GetSound(soundID), position);
        }

        public async Task LoadSound(string soundID, Uri uri) {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            var buffer = await response.Content.ReadAsBufferAsync();
            if(response.Content.Headers["Content-Type"] != "audio/wav") {
                return;
            }

            var memBuffer = new MemoryBuffer(buffer.ToArray());
            Sound sound = new Sound();
            if(!sound.LoadWav(memBuffer)) {
                return;
            }

            sound.Name = soundID;
            Application.ResourceCache.AddManualResource(sound);
        }

        private Node CreateSoundNode(Vector3? position = null) {
            var soundNode = Node.CreateChild();
            if(position.HasValue) {
                soundNode.Position = position.Value;
            }
            return soundNode;
        }
    }
}

