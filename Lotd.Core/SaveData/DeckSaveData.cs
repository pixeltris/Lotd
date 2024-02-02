using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd
{
    public class DeckSaveData : DeckSaveDataBase
    {
        public DeckSaveData(GameSaveData owner)
            : base(owner)
        {
        }

        public override void Load(BinaryReader reader)
        {
            LoadDeckData(reader);
        }

        public override void Save(BinaryWriter writer)
        {
            SaveDeckData(writer);
        }
    }

    public abstract class DeckSaveDataBase : SaveDataChunk
    {
        public string DeckName { get; set; }
        public List<short> MainDeckCards { get; private set; }
        public List<short> SideDeckCards { get; private set; }
        public List<short> ExtraDeckCards { get; private set; }

        /// <summary>
        /// Avatar / character id for this deck
        /// </summary>
        public int DeckAvatarId { get; set; }

        /// <summary>
        /// True if the deck is complete. If false the deck will be displayed but you wont be able to interact with it.
        /// - Should be named IsDeckCreated?
        /// </summary>
        public bool IsDeckComplete { get; set; }

        private const int maxMainDeckCards = 60;
        private const int maxSideDeckCards = 15;
        private const int maxExtraDeckCards = 15;

        public DeckSaveDataBase(GameSaveData owner)
            : base(owner)
        {
            MainDeckCards = new List<short>();
            SideDeckCards = new List<short>();
            ExtraDeckCards = new List<short>();
        }

        public override void Clear()
        {
            ClearDeckData();
        }

        protected void ClearDeckData()
        {
            DeckName = null;
            MainDeckCards.Clear();
            SideDeckCards.Clear();
            ExtraDeckCards.Clear();
            DeckAvatarId = 0;
            IsDeckComplete = false;
        }

        protected void LoadDeckData(BinaryReader reader)
        {
            DeckName = Encoding.Unicode.GetString(reader.ReadBytes(Constants.DeckNameByteLen)).TrimEnd('\0');

            short numMainDeckCards = reader.ReadInt16();
            short numSideDeckCards = reader.ReadInt16();
            short numExtraDeckCards = reader.ReadInt16();

            for (int i = 0; i < maxMainDeckCards; i++)
            {
                short cardId = reader.ReadInt16();
                if (cardId > 0)
                {
                    MainDeckCards.Add(cardId);
                }
            }

            for (int i = 0; i < maxSideDeckCards; i++)
            {
                short cardId = reader.ReadInt16();
                if (cardId > 0)
                {
                    SideDeckCards.Add(cardId);
                }
            }

            for (int i = 0; i < maxExtraDeckCards; i++)
            {
                short cardId = reader.ReadInt16();
                if (cardId > 0)
                {
                    ExtraDeckCards.Add(cardId);
                }
            }

            reader.ReadBytes(12);//some kind of unique deck id? (8+4 bytes?)
            reader.ReadBytes(12);//some kind of unique deck id? (8+4 bytes?)
            reader.ReadUInt32();//0?
            reader.ReadUInt32();//0?
            reader.ReadUInt32();//0?
            DeckAvatarId = reader.ReadInt32();
            reader.ReadUInt32();//0?
            reader.ReadUInt32();//0?
            IsDeckComplete = reader.ReadUInt32() == 1;// must be 1 to be complete
        }

        protected void SaveDeckData(BinaryWriter writer)
        {
            writer.Write(Encoding.Unicode.GetBytes(DeckName, Constants.DeckNameByteLen, Constants.DeckNameUsableLen));

            writer.Write((short)(Math.Min(maxMainDeckCards, MainDeckCards.Count)));
            writer.Write((short)(Math.Min(maxSideDeckCards, SideDeckCards.Count)));
            writer.Write((short)(Math.Min(maxExtraDeckCards, ExtraDeckCards.Count)));

            for (int i = 0; i < maxMainDeckCards; i++)
            {
                short cardId = 0;
                if (MainDeckCards.Count > i)
                {
                    cardId = MainDeckCards[i];
                }
                writer.Write(cardId);
            }

            for (int i = 0; i < maxSideDeckCards; i++)
            {
                short cardId = 0;
                if (SideDeckCards.Count > i)
                {
                    cardId = SideDeckCards[i];
                }
                writer.Write(cardId);
            }

            for (int i = 0; i < maxExtraDeckCards; i++)
            {
                short cardId = 0;
                if (ExtraDeckCards.Count > i)
                {
                    cardId = ExtraDeckCards[i];
                }
                writer.Write(cardId);
            }

            writer.Write(new byte[12]);
            writer.Write(new byte[12]);
            writer.Write((uint)0);
            writer.Write((uint)0);
            writer.Write((uint)0);
            writer.Write(DeckAvatarId);
            writer.Write((uint)0);
            writer.Write((uint)0);
            writer.Write((uint)(IsDeckComplete ? 1 : 0));
        }
    }
}
