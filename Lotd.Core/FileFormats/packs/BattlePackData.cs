using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd.FileFormats
{
    /// <summary>
    /// Holds card information for battle packs "Play using a temporary Deck made from Battle Packs." (Sealed Play / Draft Play)
    /// </summary>
    public class BattlePackData : FileData
    {
        // This is part of the zib file "packs.zib"
        // Path: "packs\bpack_BattlePack1.bin"

        public List<CardCollection> Categories { get; private set; }

        public BattlePackData()
        {
            Categories = new List<CardCollection>();
        }

        public override void Load(BinaryReader reader, long length)
        {
            Categories.Clear();
            long fileStartPos = reader.BaseStream.Position;

            long numCategories = reader.ReadInt64();
            for (int i = 0; i < numCategories; i++)
            {
                long cardListOffset = reader.ReadInt64();

                long tempOffset = reader.BaseStream.Position;

                reader.BaseStream.Position = fileStartPos + cardListOffset;
                short cardCount = reader.ReadInt16();
                CardCollection cardCollection = new CardCollection();
                for (int j = 0; j < cardCount; j++)
                {
                    cardCollection.Add(reader.ReadInt16());
                }
                Categories.Add(cardCollection);

                reader.BaseStream.Position = tempOffset;
            }
        }

        public override void Save(BinaryWriter writer)
        {
            long fileStartPos = writer.BaseStream.Position;

            writer.Write((long)Categories.Count);

            long offsetsOffset = writer.BaseStream.Position;
            writer.Write(new byte[Categories.Count * 8]);

            for(int i = 0; i < Categories.Count; i++)
            {
                long tempOffset = writer.BaseStream.Position;

                writer.BaseStream.Position = offsetsOffset + (i * 8);
                writer.WriteOffset(fileStartPos, tempOffset);
                writer.BaseStream.Position = tempOffset;

                writer.Write((short)Categories[i].CardIds.Count);
                foreach (short cardId in Categories[i].CardIds)
                {
                    writer.Write(cardId);
                }
            }
        }
    }
}
