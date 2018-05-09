using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd.FileFormats
{
    /// <summary>
    /// Holds card information about a given pack which can be bought from the shop (grandpa, kaiba, etc)
    /// </summary>
    public class ShopPackData : FileData
    {
        // This is part of the zib file "packs.zib"
        // Path: "packs\packdata_1_1.bin"

        // Note that the file name determines which "series" this pack belongs to such as "Yu-Gi-Oh!"
        // packdata_1_4 where '1' is "Yu-Gi-Oh!" and '4' is Joey
        // 1 = Yu-Gi-Oh!
        // 2 = Yu-Gi-Oh! GX
        // 3 = Yu-Gi-Oh! 5D's
        // 4 = Yu-Gi-Oh! ZEXAL
        // 5 = Yu-Gi-Oh! ARC-V

        public CardCollection CommonCards { get; set; }// Commons?
        public CardCollection RareCards { get; set; }// Rares? (the last card that flips)

        public ShopPackData()
        {
            CommonCards = new CardCollection();
            RareCards = new CardCollection();
        }

        public override void Load(BinaryReader reader, long length)
        {
            CommonCards.Clear();
            RareCards.Clear();

            short commonCardCount = reader.ReadInt16();
            short rareCardCount = reader.ReadInt16();

            for (int i = 0; i < commonCardCount; i++)
            {
                CommonCards.Add(reader.ReadInt16());
            }

            for (int i = 0; i < rareCardCount; i++)
            {
                RareCards.Add(reader.ReadInt16());
            }
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write((short)(CommonCards == null ? 0 : CommonCards.CardIds.Count));
            writer.Write((short)(RareCards == null ? 0 : RareCards.CardIds.Count));

            if (CommonCards != null)
            {
                CommonCards.Sort();
                foreach (short cardId in CommonCards.CardIds)
                {
                    writer.Write(cardId);
                }
            }

            if (RareCards != null)
            {
                RareCards.Sort();
                foreach (short cardId in RareCards.CardIds)
                {
                    writer.Write(cardId);
                }
            }
        }
    }
}
