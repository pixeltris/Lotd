using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd.FileFormats
{
    public class Credits : FileData
    {
        public string Text { get; set; }
        public readonly Encoding CreditsEncoding = Encoding.Unicode;

        public override void Load(BinaryReader reader, long length)
        {
            Text = CreditsEncoding.GetString(reader.ReadBytes(length));
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(CreditsEncoding.GetBytes(Text == null ? string.Empty : Text));
        }
    }
}
