using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd.FileFormats
{
    public class CharData : FileData
    {
        static Encoding keyEncoding = Encoding.ASCII;
        static Encoding valueEncoding = Encoding.Unicode;
        static Encoding descriptionEncoding = Encoding.Unicode;
        public Dictionary<int, Item> Items { get; private set; }

        public override bool IsLocalized
        {
            get { return true; }
        }

        public CharData()
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
                int challengeDeckId = reader.ReadInt32();
                int unk3 = reader.ReadInt32();
                int dlcId = reader.ReadInt32();
                int unk5 = reader.ReadInt32();
                long type = reader.ReadInt64();
                long keyOffset = reader.ReadInt64();
                long valueOffset = reader.ReadInt64();
                long descriptionOffset = reader.ReadInt64();

                long tempOffset = reader.BaseStream.Position;

                reader.BaseStream.Position = fileStartPos + keyOffset;
                string codeName = reader.ReadNullTerminatedString(keyEncoding);

                reader.BaseStream.Position = fileStartPos + valueOffset;
                string name = reader.ReadNullTerminatedString(valueEncoding);

                reader.BaseStream.Position = fileStartPos + descriptionOffset;
                string bio = reader.ReadNullTerminatedString(valueEncoding);

                reader.BaseStream.Position = tempOffset;

                Item item;
                if (!Items.TryGetValue(id, out item))
                {
                    item = new Item(id, series, challengeDeckId, unk3, dlcId, unk5, type);
                    Items.Add(item.Id, item);
                }
                item.CodeName.SetText(language, codeName);
                item.Name.SetText(language, name);
                item.Bio.SetText(language, bio);
            }
        }

        public override void Save(BinaryWriter writer, Language language)
        {
            int firstChunkItemSize = 56;// Size of each item in the first chunk
            long fileStartPos = writer.BaseStream.Position;

            writer.Write((ulong)Items.Count);

            long offsetsOffset = writer.BaseStream.Position;
            writer.Write(new byte[Items.Count * firstChunkItemSize]);

            int index = 0;
            foreach(Item item in Items.Values)
            {
                int keyLen = GetStringSize(item.CodeName.GetText(language), keyEncoding);
                int valueLen = GetStringSize(item.Name.GetText(language), valueEncoding);
                long tempOffset = writer.BaseStream.Position;

                writer.BaseStream.Position = offsetsOffset + (index * firstChunkItemSize);
                writer.Write(item.Id);
                writer.Write((int)item.Series);
                writer.Write(item.ChallengeDeckId);
                writer.Write(item.Unk3);
                writer.Write(item.DlcId);
                writer.Write(item.Unk5);
                writer.Write(item.Type);
                writer.WriteOffset(fileStartPos, tempOffset);
                writer.WriteOffset(fileStartPos, tempOffset + keyLen);
                writer.WriteOffset(fileStartPos, tempOffset + keyLen + valueLen);
                writer.BaseStream.Position = tempOffset;

                writer.WriteNullTerminatedString(item.CodeName.GetText(language), keyEncoding);
                writer.WriteNullTerminatedString(item.Name.GetText(language), valueEncoding);
                writer.WriteNullTerminatedString(item.Bio.GetText(language), descriptionEncoding);

                index++;
            }
        }

        public override void Clear()
        {
            Items.Clear();
        }

        public class Item
        {
            public int Id { get; set; }
            public DuelSeries Series { get; set; }
            public int ChallengeDeckId { get; set; }
            public int Unk3 { get; set; }
            public int DlcId { get; set; }
            public int Unk5 { get; set; }
            public long Type { get; set; }

            /// <summary>
            /// The code name for this character. This is the name that can be found in /busts/
            /// </summary>
            public LocalizedText CodeName { get; set; }

            /// <summary>
            /// The display name of the character
            /// </summary>
            public LocalizedText Name { get; set; }

            /// <summary>
            /// An unused character bio. Most characters don't have this and it doesn't appear in game
            /// </summary>
            public LocalizedText Bio { get; set; }

            public Item(int id, DuelSeries series, int challengeDeckId, int unk3, int dlcId, int unk5, long type)
            {
                Id = id;
                Series = series;
                ChallengeDeckId = challengeDeckId;
                Unk3 = unk3;
                DlcId = dlcId;
                Unk5 = unk5;
                Type = type;
                CodeName = new LocalizedText();
                Name = new LocalizedText();
                Bio = new LocalizedText();
            }

            public override string ToString()
            {
                return "id: " + Id + " series: " + Series + " challengeDeckId: " + ChallengeDeckId + " unk3: " + Unk3 + " dlcId: " + DlcId + " unk5: " + Unk5 +
                   " type: " + Type + " codeName: '" + CodeName + "' name: '" + Name + "' bio: '" + Bio + "'";
            }
        }
    }
}
