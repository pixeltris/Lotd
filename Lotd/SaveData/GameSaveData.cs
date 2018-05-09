using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd
{
    public partial class GameSaveData
    {
        const uint headerMagic1 = 0x54CE29F9;// 1422797305
        const uint headerMagic2 = 0x04714D02;// 74534146
        public const int FileLength = 29005;

        // Offsets - these are safe to be hard coded as the layout of the file is always the same
        // First 20 bytes = magic + signature + play count
        public const int UnkOffset1 = 20;// int + int + int + int - default values are (5 / 5 / 0 / 0x3F800000)
        public const int StatsOffset = 36;
        public const int BattlePacksOffset = 380;
        public const int MiscDataOffset = 3600;
        public const int CampaignDataOffset = 5648;
        public const int DecksOffset = 11696;
        public const int CardListOffset = 21424;

        public static int StatsSize
        {
            get { return BattlePacksOffset - StatsOffset; }
        }
        public static int BattlePacksSize
        {
            get { return MiscDataOffset - BattlePacksOffset; }
        }
        public static int MiscDataSize
        {
            get { return CampaignDataOffset - MiscDataOffset; }
        }
        public static int CampaignDataSize
        {
            get { return DecksOffset - CampaignDataOffset; }
        }
        public static int DecksSize
        {
            get { return CardListOffset - DecksOffset; }
        }
        public static int CardListSize
        {
            get { return FileLength - CardListOffset; }
        }

        /// <summary>
        /// Number of times the game has been played - increases once per play session.
        /// Note that if game save data doesn't save this value wont increase for that play session.
        /// </summary>
        public int PlayCount { get; set; }

        public StatSaveData Stats { get; set; }
        public BattlePackSaveData[] BattlePacks { get; private set; }
        public MiscSaveData Misc { get; set; }
        public CampaignSaveData Campaign { get; set; }
        public DeckSaveData[] Decks { get; private set; }
        public CardListSaveData CardList { get; private set; }

        public GameSaveData()
        {
            Clear();
        }

        public void Clear()
        {
            if (Stats == null)
            {
                Stats = new StatSaveData();
            }
            Stats.Clear();

            if (BattlePacks == null || BattlePacks.Length != Constants.NumBattlePacks)
            {
                BattlePacks = new BattlePackSaveData[Constants.NumBattlePacks];
            }
            for (int i = 0; i < Constants.NumBattlePacks; i++)
            {
                if (BattlePacks[i] == null)
                {
                    BattlePacks[i] = new BattlePackSaveData();
                }
                BattlePacks[i].Clear();
            }

            if (Misc == null)
            {
                Misc = new MiscSaveData();
            }
            Misc.Clear();

            if (Campaign == null)
            {
                Campaign = new CampaignSaveData();
            }
            Campaign.Clear();

            if (Decks == null || Decks.Length != Constants.NumUserDecks)
            {
                Decks = new DeckSaveData[Constants.NumUserDecks];
            }
            for (int i = 0; i < Constants.NumUserDecks; i++)
            {
                if (Decks[i] == null)
                {
                    Decks[i] = new DeckSaveData();
                }
                Decks[i].Clear();
            }

            if (CardList == null)
            {
                CardList = new CardListSaveData();
            }
            CardList.Clear();
        }

        public bool Load()
        {
            return Load(GetSaveFilePath());
        }

        public bool Load(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return false;
            }
            return Load(File.ReadAllBytes(path), true);
        }

        public bool Load(byte[] buffer)
        {
            return Load(buffer, false);
        }

        private bool Load(byte[] buffer, bool checkSignature)
        {
            Clear();

            try
            {
                using (BinaryReader reader = new BinaryReader(new MemoryStream(buffer)))
                {
                    Debug.Assert(reader.ReadUInt32() == headerMagic1, "Bad header magic");
                    Debug.Assert(reader.ReadUInt32() == headerMagic2, "Bad header magic");

                    uint fileLength = reader.ReadUInt32();
                    Debug.Assert(fileLength == buffer.Length, "Bad save data length");
                    Debug.Assert(fileLength == FileLength, "Bad save data length");

                    uint signature = reader.ReadUInt32();
                    if (checkSignature)
                    {
                        uint calculatedSignature = GetSignature(buffer);
                        Debug.Assert(calculatedSignature == signature, "Bad save data signature");
                    }

                    PlayCount = reader.ReadInt32();

                    Debug.Assert(reader.BaseStream.Position == UnkOffset1);
                    uint dataMagic1 = reader.ReadUInt32();
                    uint dataMagic2 = reader.ReadUInt32();
                    uint dataMagic3 = reader.ReadUInt32();
                    uint dataMagic4 = reader.ReadUInt32();
                    if ((dataMagic1 != 5 || dataMagic2 != 5 || dataMagic3 != 0 || dataMagic4 != 0x3F800000) &&
                        Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }

                    Debug.Assert(reader.BaseStream.Position == StatsOffset);
                    Stats.Load(reader);

                    Debug.Assert(reader.BaseStream.Position == BattlePacksOffset);
                    for (int i = 0; i < Constants.NumBattlePacks; i++)
                    {
                        BattlePacks[i].Load(reader);
                    }

                    Debug.Assert(reader.BaseStream.Position == MiscDataOffset);
                    Misc.Load(reader);

                    Debug.Assert(reader.BaseStream.Position == CampaignDataOffset);
                    Campaign.Load(reader);

                    Debug.Assert(reader.BaseStream.Position == DecksOffset);
                    for (int i = 0; i < Constants.NumUserDecks; i++)
                    {
                        Decks[i].Load(reader);
                    }

                    Debug.Assert(reader.BaseStream.Position == CardListOffset);
                    CardList.Load(reader);

                    Debug.Assert(reader.BaseStream.Position == FileLength);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Save()
        {
            Save(GetSaveFilePath());
        }

        public void Save(string path)
        {
            byte[] buffer = ToArray();
            if (buffer != null)
            {
                File.WriteAllBytes(path, buffer);
            }
        }

        public byte[] ToArray()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(headerMagic1);
                writer.Write(headerMagic2);
                writer.Write(FileLength);
                writer.Write((uint)0);// signature to be written later
                writer.Write(PlayCount);

                Debug.Assert(writer.BaseStream.Position == UnkOffset1);
                // Unknown data - use the default values
                writer.Write((uint)5);
                writer.Write((uint)5);
                writer.Write((uint)0);
                writer.Write((uint)0x3F800000);

                Debug.Assert(writer.BaseStream.Position == StatsOffset);
                (Stats != null ? Stats : new StatSaveData()).Save(writer);

                Debug.Assert(writer.BaseStream.Position == BattlePacksOffset);
                for (int i = 0; i < Constants.NumBattlePacks; i++)
                {
                    (BattlePacks[i] != null ? BattlePacks[i] : new BattlePackSaveData()).Save(writer);
                }

                Debug.Assert(writer.BaseStream.Position == MiscDataOffset);
                (Misc != null ? Misc : new MiscSaveData()).Save(writer);

                Debug.Assert(writer.BaseStream.Position == CampaignDataOffset);
                (Campaign != null ? Campaign : new CampaignSaveData()).Save(writer);

                Debug.Assert(writer.BaseStream.Position == DecksOffset);
                for (int i = 0; i < Constants.NumUserDecks; i++)
                {
                    (Decks[i] != null ? Decks[i] : new DeckSaveData()).Save(writer);
                }

                Debug.Assert(writer.BaseStream.Position == CardListOffset);
                (CardList != null ? CardList : new CardListSaveData()).Save(writer);

                Debug.Assert(writer.BaseStream.Length == FileLength);

                byte[] buffer = stream.ToArray();
                SaveSignature(buffer);
                return buffer;
            }
        }
    }
}
