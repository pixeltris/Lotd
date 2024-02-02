using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd
{
    public class BattlePackSaveData : DeckSaveDataBase
    {
        public BattlePackSaveState State { get; set; }

        public int NumDuelsCompleted { get; set; }
        public int NumDuelsWon { get; set; }
        public int NumDuelsLost { get; set; }

        public BattlePackDuelResult DuelResult1 { get; set; }
        public BattlePackDuelResult DuelResult2 { get; set; }
        public BattlePackDuelResult DuelResult3 { get; set; }
        public BattlePackDuelResult DuelResult4 { get; set; }
        public BattlePackDuelResult DuelResult5 { get; set; }

        // A flat list of all cards you drafted
        public List<short> DraftedCards { get; private set; }

        public List<short> SealedCards1 { get; private set; }
        public List<short> SealedCards2 { get; private set; }
        public List<short> SealedCards3 { get; private set; }

        // This is seemingly unused? It likely uses the index instead to work out which battle pack it belongs to.
        public BattlePackType Type { get; set; }
        
        public byte Unk1 { get; set; }
        public int Unk2 { get; set; }
        public int Unk3 { get; set; }
        public int Unk4 { get; set; }
        public int Unk5 { get; set; }
        public byte Unk6 { get; set; }
        
        public const int NumDraftedCards = 90;
        public const int NumSealedCardsPerSegment = 15;

        public BattlePackSaveData(GameSaveData owner)
            : base(owner)
        {
            DraftedCards = new List<short>();
            SealedCards1 = new List<short>();
            SealedCards2 = new List<short>();
            SealedCards3 = new List<short>();
        }

        public override void Clear()
        {
            base.Clear();

            State = BattlePackSaveState.None;

            NumDuelsCompleted = 0;
            NumDuelsWon = 0;
            NumDuelsLost = 0;

            DuelResult1 = BattlePackDuelResult.None;
            DuelResult2 = BattlePackDuelResult.None;
            DuelResult3 = BattlePackDuelResult.None;
            DuelResult4 = BattlePackDuelResult.None;
            DuelResult5 = BattlePackDuelResult.None;

            DraftedCards.Clear();

            SealedCards1.Clear();
            SealedCards2.Clear();
            SealedCards3.Clear();

            // This isn't really correct, should be defaulted to the battle pack index
            Type = default(BattlePackType);
        }

        public override void Load(BinaryReader reader)
        {
            Unk1 = reader.ReadByte();// always 0?
            State = (BattlePackSaveState)reader.ReadInt32();
            NumDuelsCompleted = reader.ReadInt32();
            NumDuelsWon = reader.ReadInt32();
            NumDuelsLost = reader.ReadInt32();
            Unk2 = reader.ReadInt32();// always 0?
            DuelResult1 = (BattlePackDuelResult)reader.ReadInt32();
            DuelResult2 = (BattlePackDuelResult)reader.ReadInt32();
            DuelResult3 = (BattlePackDuelResult)reader.ReadInt32();
            DuelResult4 = (BattlePackDuelResult)reader.ReadInt32();
            DuelResult5 = (BattlePackDuelResult)reader.ReadInt32();

            LoadDeckData(reader);

            // Drafted - "Battle Pack: Epic Dawn" - 0
            // Sealed - "Battle Pack: Epic Dawn" - 1
            // Drafted - "Battle Pack 2: War of the Giants" - 2
            // Sealed - "Battle Pack 2: War of the Giants" - 3
            // Drafted - "Battle Pack 2: War of the Giants - Round 2" - 4
            Type = (BattlePackType)reader.ReadInt32();

            for (int i = 0; i < NumDraftedCards; i++)
            {
                // Uses -1 instead of 0 here for null card (45 -1 cards for draft play which has 45 cards not 90)
                // In draft play this list isn't filled until the drafting is complete. Before it is complete it is all 0.
                short cardId = reader.ReadInt16();
                if (cardId > 0)
                {
                    DraftedCards.Add(cardId);
                }
            }

            Unk3 = reader.ReadInt32();// Drafted - 0x2D
            Unk4 = reader.ReadInt32();// Drafted - 0x03
            Unk5 = reader.ReadInt32();// 0?
            Unk6 = reader.ReadByte();// bool has sealed data?

            // No idea what these cards are used for. They don't appear in the generated deck or the AI decks.
            LoadSealedCards(reader, SealedCards1);
            LoadSealedCards(reader, SealedCards2);
            LoadSealedCards(reader, SealedCards3);
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(Unk1);
            writer.Write((int)State);
            writer.Write(NumDuelsCompleted);
            writer.Write(NumDuelsWon);
            writer.Write(NumDuelsLost);
            writer.Write(Unk2);
            writer.Write((int)DuelResult1);
            writer.Write((int)DuelResult2);
            writer.Write((int)DuelResult3);
            writer.Write((int)DuelResult4);
            writer.Write((int)DuelResult5);

            SaveDeckData(writer);

            writer.Write((int)Type);

            for (int i = 0; i < NumDraftedCards; i++)
            {
                short cardId = (short)(State == BattlePackSaveState.Created ? -1 : 0);
                if (DraftedCards.Count > i)
                {
                    cardId = DraftedCards[i];
                }
                writer.Write(cardId);
            }

            writer.Write(Unk3);
            writer.Write(Unk4);
            writer.Write(Unk5);
            writer.Write(Unk6);
            
            SaveSealedCards(writer, SealedCards1);
            SaveSealedCards(writer, SealedCards2);
            SaveSealedCards(writer, SealedCards3);
        }

        private void LoadSealedCards(BinaryReader reader, List<short> sealedCards)
        {
            int numSealedCards = reader.ReadInt32();
            for (int i = 0; i < NumSealedCardsPerSegment; i++)
            {
                short cardId = reader.ReadInt16();
                if (cardId > 0)
                {
                    sealedCards.Add(cardId);
                }
            }
        }

        private void SaveSealedCards(BinaryWriter writer, List<short> sealedCards)
        {
            writer.Write(Math.Min(NumSealedCardsPerSegment, sealedCards.Count));
            for (int i = 0; i < NumSealedCardsPerSegment; i++)
            {
                short cardId = 0;
                if (sealedCards.Count > i)
                {
                    cardId = sealedCards[i];
                }
                writer.Write(cardId);
            }
        }
    }

    public enum BattlePackSaveState
    {
        None = 0,// Battle pack doesn't exist / not created / not drafting
        Created = 1,
        Drafting = 2
    }

    public enum BattlePackDuelResult
    {
        None = 0,
        Won = 1,
        Lost = 2,
        Draw = 3
    }

    public enum BattlePackType
    {
        Drafted_BattlePackEpicDawn = 0,
        Sealed_BattlePackEpicDawn = 1,
        Drafted_BattlePack2WaroftheGiants = 2,
        Sealed_BattlePack2WaroftheGiants = 3,
        Drafted_BattlePack2WaroftheGiantsRound2 = 4
    }
}
