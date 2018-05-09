using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd.FileFormats
{
    /// <summary>
    /// This is found under "Help & Options" -> "How to Play"
    /// </summary>
    public class HowToPlay : FileData
    {
        const int align = 8;
        Encoding stringEncoding = Encoding.BigEndianUnicode;
        public List<Entry> Entries { get; private set; }

        public HowToPlay()
        {
            Entries = new List<Entry>();
        }

        public override void Load(BinaryReader reader, long length)
        {
            Entries.Clear();
            long dataOffset = reader.BaseStream.Position;
            
            uint count = Endian.ConvertUInt32(reader.ReadUInt32());

            // This alignment will always happen as each entry is 8 bytes each and the entry count is 4 bytes long.
            // Therefore there should always be 4 bytes of padding between the entry and string data.
            uint startChunkLen = (count * 8) + 4;
            if (startChunkLen % align != 0)
            {
                startChunkLen += align - startChunkLen % align;
            }

            long stringOffset = dataOffset + startChunkLen;

            for (int i = 0; i < count; i++)
            {
                // The native code writes to these palceholder bytes to calculate the offset of the string at runtime.
                // These are always expected to be be zero.
                uint placeholderBytes = reader.ReadUInt32();
                Debug.Assert(placeholderBytes == 0, "Unexpected placeholder data in howtoplay bin file");

                EntryType type = (EntryType)reader.ReadByte();
                byte imageId = reader.ReadByte();
                ushort len = Endian.ConvertUInt16(reader.ReadUInt16());

                long tempOffset = reader.BaseStream.Position;

                reader.BaseStream.Position = stringOffset;
                string str = Encoding.BigEndianUnicode.GetString(reader.ReadBytes(len * 2));
                reader.BaseStream.Position = tempOffset;

                Entry entry = new Entry(str, type, imageId);
                Entries.Add(entry);
                
                stringOffset += (len * 2) + 2;// Skip the null terminator bytes
            }
        }

        public override void Save(BinaryWriter writer)
        {
            // Note that the order of the entries is likely very important
            // MainEntry
            //  SubEntry
            //   SubEntryItem
            //    Description
            //   SubEntryItem
            //    Description
            // MainEntry
            // ...

            uint count = (uint)Entries.Count;

            bool injectEndEntry = false;
            if (Entries.Count == 0 || Entries[Entries.Count - 1].Type != EntryType.End)
            {
                count++;
                injectEndEntry = true;
            }

            writer.Write(Endian.ConvertUInt32(count));

            foreach (Entry entry in Entries)
            {
                writer.Write((uint)0);
                writer.Write((byte)entry.Type);
                writer.Write(entry.ImageId);
                writer.Write(Endian.ConvertUInt16((ushort)(entry.Text == null ? 0 : entry.Text.Length)));
            }
            if (injectEndEntry)
            {
                writer.Write((uint)0);
                writer.Write((byte)EntryType.End);
                writer.Write((byte)0);
                writer.Write((ushort)0);
            }

            // Add alignment padding
            uint startChunkLen = (count * 8) + 4;
            if (startChunkLen % align != 0)
            {
                writer.Write(new byte[align - startChunkLen % align]);
            }

            foreach (Entry entry in Entries)
            {
                writer.WriteNullTerminatedString(entry.Text, stringEncoding);
            }
            if (injectEndEntry)
            {                
                writer.Write((ushort)0);// Empty string
            }
        }

        public class Entry
        {
            public string Text { get; set; }
            public EntryType Type { get; set; }

            /// <summary>
            /// Image id for the image attached at the bottom of the text.
            /// The image for this id can be found under main/howto_img/help_duelimg_XXX.png (always 3 digits, padded left with '0')
            /// </summary>
            public byte ImageId { get; set; }

            public Entry(string text, EntryType type, byte imageId)
            {
                Text = text;
                Type = type;
                ImageId = imageId;                
            }
        }

        public enum EntryType : byte
        {
            /// <summary>
            /// Main entry / chapter under "Chapter list" e.g. "Duel Basics"
            /// </summary>
            MainEntry = 0,// 

            /// <summary>
            /// Sub entry under a main entry e.g. "●What is a "Duel"?"
            /// </summary>
            SubEntry = 1,

            /// <summary>
            /// Appears directly under a sub entry e.g. "Duel Types" under "●What is a "Duel"?"
            /// </summary>
            SubEntryItem = 2,

            Unknown = 3,

            /// <summary>
            /// The last entry seems to be 4 with an empty string ""
            /// </summary>
            End = 4,

            /// <summary>
            /// The description text
            /// </summary>
            Description = 0xFF
        }
    }
}
