using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd.FileFormats
{
    /// <summary>
    /// Note that this structure doesn't contain the background file information under /arenas/ so it is likely
    /// impossible to add new arenas with new background images.
    /// </summary>
    public class ArenaData : FileData
    {
        static Encoding keyEncoding = Encoding.ASCII;
        static Encoding valueEncoding = Encoding.Unicode;
        static Encoding value2Encoding = Encoding.Unicode;
        public Dictionary<int, Item> Items { get; private set; }

        public override bool IsLocalized
        {
            get { return true; }
        }

        public ArenaData()
        {
            Items = new Dictionary<int, Item>();
        }

        public override void Load(BinaryReader reader, long length, Language language)
        {
            long fileStartPos = reader.BaseStream.Position;

            uint count = (uint)reader.ReadUInt64();
            for (uint i = 0; i < count; i++)
            {
                int id = reader.ReadInt32();
                long keyOffset = reader.ReadInt64();
                long valueOffset = reader.ReadInt64();
                long value2Offset = reader.ReadInt64();

                long tempOffset = reader.BaseStream.Position;

                reader.BaseStream.Position = fileStartPos + keyOffset;
                string key = reader.ReadNullTerminatedString(keyEncoding);

                reader.BaseStream.Position = fileStartPos + valueOffset;
                string value = reader.ReadNullTerminatedString(valueEncoding);

                reader.BaseStream.Position = fileStartPos + value2Offset;
                string value2 = reader.ReadNullTerminatedString(valueEncoding);

                reader.BaseStream.Position = tempOffset;

                Item item;
                if (!Items.TryGetValue(id, out item))
                {
                    item = new Item(id);
                    Items.Add(item.Id, item);
                }
                item.Key.SetText(language, key);
                item.Value.SetText(language, value);
                item.Value2.SetText(language, value2);
            }
        }

        public override void Save(BinaryWriter writer, Language language)
        {
            int firstChunkItemSize = 28;// Size of each item in the first chunk
            long fileStartPos = writer.BaseStream.Position;

            writer.Write((ulong)Items.Count);

            long offsetsOffset = writer.BaseStream.Position;
            writer.Write(new byte[Items.Count * firstChunkItemSize]);

            int index = 0;
            foreach(Item item in Items.Values)
            {
                int keyLen = GetStringSize(item.Key.GetText(language), keyEncoding);
                int valueLen = GetStringSize(item.Value.GetText(language), valueEncoding);
                long tempOffset = writer.BaseStream.Position;

                writer.BaseStream.Position = offsetsOffset + (index * firstChunkItemSize);
                writer.Write(item.Id);
                writer.WriteOffset(fileStartPos, tempOffset);
                writer.WriteOffset(fileStartPos, tempOffset + keyLen);
                writer.WriteOffset(fileStartPos, tempOffset + keyLen + valueLen);
                writer.BaseStream.Position = tempOffset;

                writer.WriteNullTerminatedString(item.Key.GetText(language), keyEncoding);
                writer.WriteNullTerminatedString(item.Value.GetText(language), valueEncoding);
                writer.WriteNullTerminatedString(item.Value2.GetText(language), value2Encoding);

                index++;
            }
        }

        public class Item
        {
            public int Id { get; set; }
            public LocalizedText Key { get; set; }
            public LocalizedText Value { get; set; }
            public LocalizedText Value2 { get; set; }

            public Item(int id)
            {
                Id = id;
                Key = new LocalizedText();
                Value = new LocalizedText();
                Value2 = new LocalizedText();
            }

            public override string ToString()
            {
                return "id: " + Id + " key: '" + Key + "' value: '" + Value + "' value2: '" + Value2 + "'";
            }
        }
    }
}
