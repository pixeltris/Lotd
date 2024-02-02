using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd.FileFormats
{
    public class StringBnd : FileData
    {
        private Encoding encoding = Encoding.BigEndianUnicode;

        public List<LocalizedText> Strings { get; private set; }

        public override bool IsLocalized
        {
            get { return true; }
        }

        public StringBnd()
        {
            Strings = new List<LocalizedText>();
        }

        public override void Load(BinaryReader reader, long length, Language language)
        {
            long fileStartPos = reader.BaseStream.Position;

            uint count = Endian.ConvertUInt32(reader.ReadUInt32());
            for (int i = 0; i < count; i++)
            {
                uint offset = Endian.ConvertUInt32(reader.ReadUInt32());
                long tempOffset = reader.BaseStream.Position;

                reader.BaseStream.Position = fileStartPos + offset;

                LocalizedText text = Strings.Count > i ? Strings[i] : null;
                if (text == null)
                {
                    text = new LocalizedText();
                    Strings.Add(text);
                }
                text.SetText(language, reader.ReadNullTerminatedString(encoding));

                reader.BaseStream.Position = tempOffset;
            }
        }

        public override void Save(BinaryWriter writer, Language language)
        {
            long fileStartPos = writer.BaseStream.Position;

            writer.Write(Endian.ConvertUInt32((uint)Strings.Count));
            writer.Write(new byte[Strings.Count * 4]);

            for (int i = 0; i < Strings.Count; i++)
            {
                long writerOffset = writer.BaseStream.Position;
                
                // Write the offset
                writer.BaseStream.Position = fileStartPos + (4 + (i * 4));
                writer.Write(Endian.ConvertUInt32((uint)(writerOffset - fileStartPos)));

                // Write the string
                writer.BaseStream.Position = writerOffset;
                writer.WriteNullTerminatedString(Strings[i].GetText(language), encoding);
            }
        }
    }
}
