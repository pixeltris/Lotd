using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd.FileFormats
{
    /// <summary>
    /// Holds a list of related cards for each card. This information is displayed in the panel on the right side of the screen
    /// in the deck editor. This is tagdata.bin
    /// </summary>
    public class RelatedCardData : FileData
    {
        /// <summary>
        /// List of related cards.
        /// The first list represents a card index (not the card id).
        /// The second list represents the related cards for that index.
        /// </summary>
        public List<List<Item>> Items { get; private set; }

        public RelatedCardData()
        {
            Items = new List<List<Item>>();
        }

        public override void Load(BinaryReader reader, long length)
        {
            Clear();

            // There doesn't seem to be any identifier which says how many items to read. Assuming it reads until known max cards.
            int numCards = Constants.GetNumCards2(File.Archive.Version);

            long dataStart = reader.BaseStream.Position + (numCards * 8);

            for (int i = 0; i < numCards; i++)
            {
                uint shortoffset = reader.ReadUInt32();
                uint tagCount = reader.ReadUInt32();

                long tempOffset = reader.BaseStream.Position;

                long start = dataStart + (shortoffset * 4);
                reader.BaseStream.Position = start;

                List<Item> items = new List<Item>();
                for (int j = 0; j < tagCount; j++)
                {
                    items.Add(new Item(reader.ReadUInt16(), reader.ReadUInt16()));
                }
                Items.Add(items);

                reader.BaseStream.Position = tempOffset;
            }
        }

        public override void Save(BinaryWriter writer)
        {
            uint shortOffset = 0;
            for (int i = 0; i < Items.Count; i++)
            {
                writer.Write(shortOffset);
                writer.Write(Items[i].Count);
                shortOffset += (uint)Items[i].Count + 1;
            }

            for (int i = 0; i < Items.Count; i++)
            {
                foreach (Item item in Items[i])
                {
                    writer.Write(item.CardId);
                    writer.Write(item.TagIndex);
                }
                writer.Write(0);
            }
        }

        public override void Clear()
        {
            Items.Clear();
        }

        public class Item
        {
            public ushort CardId { get; set; }

            /// <summary>
            /// An index into taginfo_X.bin
            /// </summary>
            public ushort TagIndex { get; set; }

            public Item(ushort cardId, ushort tagIndex)
            {
                CardId = cardId;
                TagIndex = tagIndex;
            }
        }
    }
}
