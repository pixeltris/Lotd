using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Lotd
{
    public class CardListSaveData : SaveDataChunk
    {
        public CardState[] Cards { get; private set; }

        public CardListSaveData(GameSaveData owner)
            : base(owner)
        {
            // Have enough cards to support the latest version of the game (so that we load old, modify as new, save as new)
            Cards = new CardState[Constants.GetNumCards2(Constants.LatestVersion)];
        }

        public override void Clear()
        {
            for (int i = 0; i < Cards.Length; i++)
            {
                Cards[i] = CardState.None;
            }
        }

        public override void Load(BinaryReader reader)
        {
            int numCards = Constants.GetNumCards2(Version);
            for (int i = 0; i < numCards; i++)
            {
                byte value = reader.ReadByte();
                if (i < Cards.Length)
                {
                    Cards[i].RawValue = value;
                }
            }
        }

        public override void Save(BinaryWriter writer)
        {
            int numCards = Constants.GetNumCards2(Version);
            for (int i = 0; i < numCards; i++)
            {
                writer.Write((byte)(i < Cards.Length ? Cards[i].RawValue : 0));
            }
        }

        public void MigrateFrom(LotdArchive archive, LotdArchive otherArchive, GameSaveData other)
        {
            Dictionary<int, CardInfo> cardsById, cardsByIndex, otherCardsById, otherCardsByIndex;
            LoadCards(archive, out cardsById, out cardsByIndex);
            LoadCards(otherArchive, out otherCardsById, out otherCardsByIndex);

            // Well this doesn't work...
            for (int i = 0; i < other.CardList.Cards.Length; i++)
            {
                if (i < otherCardsByIndex.Count)
                {
                    CardInfo otherCard = otherCardsByIndex[i];
                    CardInfo card;
                    if (cardsById.TryGetValue(otherCard.CardId, out card))
                    {
                        Cards[card.Index].RawValue = other.CardList.Cards[i].RawValue;
                    }
                }
            }
        }

        private void LoadCards(LotdArchive archive, out Dictionary<int, CardInfo> cardsById, out Dictionary<int, CardInfo> cardsByIndex)
        {
            Language targetLangue = Language.English;
            Dictionary<Language, byte[]> indxByLang = archive.LoadLocalizedBuffer("CARD_Indx_", true);
            Dictionary<Language, byte[]> namesByLang = archive.LoadLocalizedBuffer("CARD_Name_", true);
            Dictionary<Language, byte[]> descriptionsByLang = archive.LoadLocalizedBuffer("CARD_Desc_", true);

            byte[] indx = indxByLang[targetLangue];
            byte[] names = namesByLang[targetLangue];
            byte[] descriptions = descriptionsByLang[targetLangue];

            List<CardInfo> cards = new List<CardInfo>();
            using (BinaryReader indxReader = new BinaryReader(new MemoryStream(indx)))
            using (BinaryReader namesReader = new BinaryReader(new MemoryStream(names)))
            using (BinaryReader descriptionsReader = new BinaryReader(new MemoryStream(descriptions)))
            {
                Dictionary<uint, string> namesByOffset = ReadStrings(namesReader);
                Dictionary<uint, string> descriptionsByOffset = ReadStrings(descriptionsReader);

                int index = 0;
                while (true)
                {
                    uint nameOffset = indxReader.ReadUInt32();
                    uint descriptionOffset = indxReader.ReadUInt32();

                    if (indxReader.BaseStream.Position >= indxReader.BaseStream.Length)
                    {
                        // The last index points to an invalid offset
                        break;
                    }

                    CardInfo card = null;
                    if (cards.Count > index)
                    {
                        card = cards[index];
                    }
                    else
                    {
                        cards.Add(card = new CardInfo(index));
                    }

                    card.Name = namesByOffset[nameOffset];
                    card.Description = descriptionsByOffset[descriptionOffset];

                    index++;
                }
            }

            using (BinaryReader reader = new BinaryReader(new MemoryStream(archive.Root.FindFile("bin/CARD_Prop.bin").LoadBuffer())))
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    CardInfo card = cards[i];
                    uint a1 = reader.ReadUInt32();
                    uint a2 = reader.ReadUInt32();

                    uint first = (a1 << 18) | ((a1 & 0x7FC000 | a1 >> 18) >> 5);

                    uint second = (((a2 & 1u) | (a2 << 21)) & 0x80000001 | ((a2 & 0x7800) | ((a2 & 0x780 | ((a2 & 0x7E) << 10)) << 8)) << 6 |
                        ((a2 & 0x38000 | ((a2 & 0x7C0000 | ((a2 & 0x7800000 | (a2 >> 8) & 0x780000) >> 9)) >> 8)) >> 1));

                    short cardId = (short)((first >> 18) & 0x3FFF);
                    card.CardId = cardId;
                }
            }

            cardsById = new Dictionary<int, CardInfo>();
            cardsByIndex = new Dictionary<int, CardInfo>();
            foreach (CardInfo card in cards)
            {
                cardsById[card.CardId] = card;
                cardsByIndex[card.Index] = card;
            }
        }

        private Dictionary<uint, string> ReadStrings(BinaryReader reader)
        {
            Dictionary<uint, string> result = new Dictionary<uint, string>();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                uint offset = (uint)reader.BaseStream.Position;
                string name = reader.ReadNullTerminatedString(Encoding.Unicode);
                result.Add(offset, name);
            }
            return result;
        }

        class CardInfo
        {
            public int Index;
            public int CardId;
            public string Name;
            public string Description;

            public CardInfo(int index)
            {
                Index = index;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CardState
    {
        public byte RawValue;

        // xxxxx111 - number of cards owned (offset:0 bits:3 mask:7)
        // xxxx1xxx - 0="NEW" 1=seen card (offset:3 bits:1 mask:1)
        // 1111xxxx - ? (offset:4 bits:4 mask:0xF)

        /// <summary>
        /// Number of cards owned (0-3)
        /// </summary>
        public byte Count
        {
            get { return (byte)(RawValue & 7); }
            set
            {
                // Mask out the existing value
                RawValue &= 0xF8;

                // Set the new value
                RawValue |= (byte)(value & 7);
            }
        }

        /// <summary>
        /// If false this card will have a "NEW" marker on it
        /// </summary>
        public bool Seen
        {
            get { return ((RawValue >> 3) & 1) != 0; }
            set
            {
                // Mask out the existing value
                RawValue &= 0xF7;

                // Set the new value
                if (value)
                {
                    RawValue |= (byte)(1 << 3);
                }
            }
        }

        public byte Unkown
        {
            get { return (byte)(RawValue >> 4); }
            set
            {
                // Mask out the existing value
                RawValue &= 0xF;

                // Set the new value
                RawValue |= (byte)((value & 0xF) << 4);
            }
        }

        public static CardState None
        {
            get { return default(CardState); }
        }

        public override string ToString()
        {
            return "Seen: " + Seen + " count: " + Count + " value: " + RawValue;
        }
    }
}
