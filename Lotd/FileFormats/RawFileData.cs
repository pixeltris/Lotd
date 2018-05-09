using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd.FileFormats
{
    public class RawFileData : FileData
    {
        public byte[] Buffer { get; set; }

        public override void Load(BinaryReader reader, long length)
        {
            Buffer = reader.ReadBytes(length);
        }

        public override void Save(BinaryWriter writer)
        {
            if (Buffer != null)
            {
                writer.Write(Buffer);
            }
        }
    }
}
