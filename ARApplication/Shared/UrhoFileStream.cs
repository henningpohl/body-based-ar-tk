using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BodyAR {
    class UrhoFileStream : Stream {
        private Urho.IO.File file;
        private uint position = 0;

        public UrhoFileStream(Urho.IO.File file) {
            this.file = file;
        }

        public override bool CanRead => file.Mode == Urho.IO.FileMode.Read | file.Mode == Urho.IO.FileMode.ReadWrite;
        public override bool CanSeek => true;
        public override bool CanWrite => file.Mode == Urho.IO.FileMode.Write | file.Mode == Urho.IO.FileMode.ReadWrite;
        public override long Length => file.Size;

        public override long Position {
            get => position;
            set {
                position = file.Seek((uint)value);
            }
        }

        public override void Flush() {
            file.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            if(offset != 0) {
                throw new NotImplementedException();
            }
            return (int)file.Read(buffer, (uint)count);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            if(origin == SeekOrigin.Begin) {
                Position = offset;
            } else if(origin == SeekOrigin.Current) {
                Position = Position + offset;
            } else if(origin == SeekOrigin.End) {
                Position = Length + offset;
            }
            return Position;
        }

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
        }
    }
}
