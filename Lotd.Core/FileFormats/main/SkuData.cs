using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd.FileFormats
{
    /// <summary>
    /// Holds DLC package names?
    /// </summary>
    public class SkuData : FileData
    {
        static Encoding keyEncoding = Encoding.ASCII;
        static Encoding valueEncoding = Encoding.Unicode;
        public Dictionary<int, Item> Items { get; private set; }

        public override bool IsLocalized
        {
            get { return true; }
        }

        public SkuData()
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
                DuelSeries series = (DuelSeries)reader.ReadInt32();
                long keyOffset = reader.ReadInt64();
                long valueOffset = reader.ReadInt64();
                
                long tempOffset = reader.BaseStream.Position;
                
                reader.BaseStream.Position = fileStartPos + keyOffset;
                string key = reader.ReadNullTerminatedString(keyEncoding);

                reader.BaseStream.Position = fileStartPos + valueOffset;
                string value = reader.ReadNullTerminatedString(valueEncoding);

                reader.BaseStream.Position = tempOffset;

                Item item;
                if (!Items.TryGetValue(id, out item))
                {
                    item = new Item(id, series);
                    Items.Add(item.Id, item);
                }
                item.Key.SetText(language, key);
                item.Value.SetText(language, value);
            }
        }

        public override void Save(BinaryWriter writer, Language language)
        {
            int firstChunkItemSize = 24;// Size of each item in the first chunk
            long fileStartPos = writer.BaseStream.Position;

            writer.Write((ulong)Items.Count);

            long offsetsOffset = writer.BaseStream.Position;
            writer.Write(new byte[Items.Count * firstChunkItemSize]);

            int index = 0;
            foreach(Item item in Items.Values)
            {
                int keyLen = GetStringSize(item.Key.GetText(language), keyEncoding);
                long tempOffset = writer.BaseStream.Position;

                writer.BaseStream.Position = offsetsOffset + (index * firstChunkItemSize);
                writer.Write(item.Id);
                writer.Write((int)item.Series);
                writer.WriteOffset(fileStartPos, tempOffset);
                writer.WriteOffset(fileStartPos, tempOffset + keyLen);
                writer.BaseStream.Position = tempOffset;

                writer.WriteNullTerminatedString(item.Key.GetText(language), keyEncoding);
                writer.WriteNullTerminatedString(item.Value.GetText(language), valueEncoding);

                index++;
            }
        }

        public class Item
        {
            public int Id { get; set; }
            public DuelSeries Series { get; set; }// The "Yu-Gi-Oh!" series this DLC belongs to e.g. GX, 5D's, ZEXAL, ARC-V
            public LocalizedText Key { get; set; }
            public LocalizedText Value { get; set; }

            public Item(int id, DuelSeries series)
            {
                Id = id;
                Series = series;
                Key = new LocalizedText();
                Value = new LocalizedText();
            }

            public override string ToString()
            {
                return "id: " + Id + " series: " + Series + " key: '" + Key + "' value: '" + Value + "'";
            }
        }
    }
}
