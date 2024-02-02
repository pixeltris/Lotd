using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd.FileFormats
{
    public class DeckData : FileData
    {
        static Encoding deckFileNameEncoding = Encoding.ASCII;
        static Encoding deckNameEncoding = Encoding.Unicode;
        static Encoding deckDescriptionEncoding = Encoding.Unicode;
        static Encoding unkStr1Encoding = Encoding.Unicode;
        public Dictionary<int, Item> Items { get; private set; }

        public override bool IsLocalized
        {
            get { return true; }
        }

        public DeckData()
        {
            Items = new Dictionary<int, Item>();
        }

        public override void Load(BinaryReader reader, long length, Language language)
        {
            long fileStartPos = reader.BaseStream.Position;

            uint count = (uint)reader.ReadUInt64();
            for (uint i = 0; i < count; i++)
            {
                int id1 = reader.ReadInt32();
                int id2 = reader.ReadInt32();
                DuelSeries series = (DuelSeries)reader.ReadInt32();
                int signatureCardId = reader.ReadInt32();
                int deckOwner = reader.ReadInt32();
                int unk1 = reader.ReadInt32();
                long deckFileNameOffset = reader.ReadInt64();
                long deckNameOffset = reader.ReadInt64();
                long deckDescriptionOffset = reader.ReadInt64();
                long unkStr1Offset = reader.ReadInt64();

                long tempOffset = reader.BaseStream.Position;

                reader.BaseStream.Position = fileStartPos + deckFileNameOffset;
                string deckFileName = reader.ReadNullTerminatedString(deckFileNameEncoding);

                reader.BaseStream.Position = fileStartPos + deckNameOffset;
                string deckName = reader.ReadNullTerminatedString(deckNameEncoding);

                reader.BaseStream.Position = fileStartPos + deckDescriptionOffset;
                string deckDescription = reader.ReadNullTerminatedString(deckDescriptionEncoding);

                reader.BaseStream.Position = fileStartPos + unkStr1Offset;
                string unkStr1 = reader.ReadNullTerminatedString(unkStr1Encoding);

                reader.BaseStream.Position = tempOffset;

                Item item;
                if (!Items.TryGetValue(id1, out item))
                {
                    item = new Item(id1, id2, series, signatureCardId, deckOwner, unk1);
                    Items.Add(item.Id1, item);
                }
                item.DeckFileName.SetText(language, deckFileName);
                item.DeckName.SetText(language, deckName);
                item.DeckDescription.SetText(language, deckDescription);
                item.UnkStr1.SetText(language, unkStr1);
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
            foreach (Item item in Items.Values)
            {
                int deckFileNameLen = GetStringSize(item.DeckFileName.GetText(language), deckFileNameEncoding);
                int deckNameLen = GetStringSize(item.DeckName.GetText(language), deckNameEncoding);
                int deckDescriptionLen = GetStringSize(item.DeckDescription.GetText(language), deckDescriptionEncoding);
                long tempOffset = writer.BaseStream.Position;

                writer.BaseStream.Position = offsetsOffset + (index * firstChunkItemSize);
                writer.Write(item.Id1);                
                writer.Write(item.Id2);
                writer.Write((int)item.Series);
                writer.Write(item.SignatureCardId);
                writer.Write(item.DeckOwnerId);
                writer.Write(item.Unk1);
                writer.WriteOffset(fileStartPos, tempOffset);
                writer.WriteOffset(fileStartPos, tempOffset + deckFileNameLen);
                writer.WriteOffset(fileStartPos, tempOffset + deckFileNameLen + deckNameLen);
                writer.WriteOffset(fileStartPos, tempOffset + deckFileNameLen + deckNameLen + deckDescriptionLen);
                writer.BaseStream.Position = tempOffset;

                writer.WriteNullTerminatedString(item.DeckFileName.GetText(language), deckFileNameEncoding);
                writer.WriteNullTerminatedString(item.DeckName.GetText(language), deckNameEncoding);
                writer.WriteNullTerminatedString(item.DeckDescription.GetText(language), deckDescriptionEncoding);
                writer.WriteNullTerminatedString(item.UnkStr1.GetText(language), unkStr1Encoding);

                index++;
            }
        }

        public override void Clear()
        {
            Items.Clear();
        }

        public class Item
        {
            public int Id1 { get; set; }            
            public int Id2 { get; set; }
            public DuelSeries Series { get; set; }
            public int SignatureCardId { get; set; }

            /// <summary>
            /// The owner of this deck as an id. This is the same id as in CharData.Item.Id. (Joey, Mai, Kaiba, etc.)
            /// </summary>
            public int DeckOwnerId { get; set; }

            public int Unk1 { get; set; }            
            public LocalizedText DeckFileName { get; set; }
            public LocalizedText DeckName { get; set; }

            /// <summary>
            /// This is usally left black. I assume it is a description of sorts. The yu-gi deck which uses exodia says
            /// "Yami's first deck depends Exodia." one of joeys says "I like dragons." - do these appear anywhere in game?
            /// </summary>
            public LocalizedText DeckDescription { get; set; }

            public LocalizedText UnkStr1 { get; set; }

            public Item(int id1, int id2, DuelSeries series, int signatureCardId, int deckOwner, int unk1)
            {
                Id1 = id1;                
                Id2 = id2;
                Series = series;
                SignatureCardId = signatureCardId;
                DeckOwnerId = deckOwner;
                Unk1 = unk1;
                DeckFileName = new LocalizedText();
                DeckName = new LocalizedText();
                DeckDescription = new LocalizedText();
                UnkStr1 = new LocalizedText();
            }

            public override string ToString()
            {
                return "id1: " + Id1 + " id2: " + Id1 + " signatureCard: " + SignatureCardId + " deckOwner: " + DeckOwnerId + " unk1: " + Unk1 +
                    " deckFileName: '" + DeckFileName + "' deckName: '" + DeckName + "' unk4: '" + DeckDescription + "' unkStr1: '" + UnkStr1 + "'";
            }

            public CharData.Item GetDeckOwner(CharData charData)
            {
                CharData.Item owner;
                charData.Items.TryGetValue(DeckOwnerId, out owner);
                return owner;
            }
        }
    }
}
