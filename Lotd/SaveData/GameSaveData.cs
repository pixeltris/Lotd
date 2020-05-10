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
        uint headerMagic2
        {
            get
            {
                switch(Version)
                {
                    case GameVersion.Lotd:
                        return 0x04714D02;// 74534146
                    case GameVersion.LinkEvolution1:
                    case GameVersion.LinkEvolution2:
                        return 0X04ABE802;// 78374914
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        public int FileLength { get { return GetFileLength(Version); } }

        // Offsets - these are safe to be hard coded as the layout of the file is always the same
        // First 20 bytes = magic + signature + play count
        const int UnkOffset1 = 20;// int + int + int + int - default values are (5 / 5 / 0 / 0x3F800000)
        const int StatsOffset = 36;
        int BattlePacksOffset { get { return GetBattlePacksOffset(Version); }  }
        int MiscDataOffset { get { return GetMiscDataOffset(Version); } }
        int CampaignDataOffset { get { return GetCampaignDataOffset(Version); } }
        int DecksOffset { get { return GetDecksOffset(Version); } }
        int CardListOffset { get { return GetCardListOffset(Version); } }

        public int StatsSize { get { return GetStatsSize(Version); } }
        public int BattlePacksSize { get { return GetBattlePacksSize(Version); } }
        public int MiscDataSize { get { return GetMiscDataSize(Version); } }
        public int CampaignDataSize { get { return GetCampaignDataSize(Version); } }
        public int DecksSize { get { return GetDecksSize(Version); } }
        public int CardListSize { get { return GetCardListSize(Version); } }

        public static int GetFileLength(GameVersion version)
        {
            switch (version)
            {
                case GameVersion.Lotd:
                    return 29005;
                case GameVersion.LinkEvolution1:
                case GameVersion.LinkEvolution2:
                    return 44008;
                default:
                    throw new NotImplementedException();
            }
        }

        public static int GetStatsOffset(GameVersion version)
        {
            return StatsOffset;
        }
        public static int GetBattlePacksOffset(GameVersion version)
        {
            switch (version)
            {
                default:
                case GameVersion.Lotd:
                    return 380;
                case GameVersion.LinkEvolution1:
                case GameVersion.LinkEvolution2:
                    return 836;
            }
        }
        public static int GetMiscDataOffset(GameVersion version)
        {
            switch (version)
            {
                default:
                case GameVersion.Lotd:
                    return 3600;
                case GameVersion.LinkEvolution1:
                case GameVersion.LinkEvolution2:
                    return 4056;
            }
        }
        public static int GetCampaignDataOffset(GameVersion version)
        {
            switch (version)
            {
                default:
                case GameVersion.Lotd:
                    return 5648;
                case GameVersion.LinkEvolution1:
                case GameVersion.LinkEvolution2:
                    return 7024;
            }
        }
        public static int GetDecksOffset(GameVersion version)
        {
            switch (version)
            {
                default:
                case GameVersion.Lotd:
                    return 11696;
                case GameVersion.LinkEvolution1:
                case GameVersion.LinkEvolution2:
                    return 14280;
            }
        }
        public static int GetCardListOffset(GameVersion version)
        {
            switch (version)
            {
                default:
                case GameVersion.Lotd:
                    return 21424;
                case GameVersion.LinkEvolution1:
                case GameVersion.LinkEvolution2:
                    return 24008;
            }
        }
        public static int GetStatsSize(GameVersion version)
        {
            return GetBattlePacksOffset(version) - GetStatsOffset(version);
        }
        public static int GetBattlePacksSize(GameVersion version)
        {
            return GetMiscDataOffset(version) - GetBattlePacksOffset(version);
        }
        public static int GetMiscDataSize(GameVersion version)
        {
            return GetCampaignDataOffset(version) - GetMiscDataOffset(version);
        }
        public static int GetCampaignDataSize(GameVersion version)
        {
            return GetDecksOffset(version) - GetCampaignDataOffset(version);
        }
        public static int GetDecksSize(GameVersion version)
        {
            return GetCardListOffset(version) - GetDecksOffset(version);
        }
        public static int GetCardListSize(GameVersion version)
        {
            return GetFileLength(version) - GetCardListOffset(version);
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

        public GameVersion Version { get; set; }

        public GameSaveData(GameVersion version)
        {
            Version = version;
            Clear();
        }

        public void Clear()
        {
            if (Stats == null)
            {
                Stats = new StatSaveData(this);
            }
            Stats.Clear();

            if (BattlePacks == null || BattlePacks.Length != Constants.NumBattlePacks)
            {
                BattlePacks = new BattlePackSaveData[Constants.NumBattlePacks];
            }
            for (int i = 0; i < BattlePacks.Length; i++)
            {
                if (BattlePacks[i] == null)
                {
                    BattlePacks[i] = new BattlePackSaveData(this);
                }
                BattlePacks[i].Clear();
            }

            if (Misc == null)
            {
                Misc = new MiscSaveData(this);
            }
            Misc.Clear();

            if (Campaign == null)
            {
                Campaign = new CampaignSaveData(this);
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
                    Decks[i] = new DeckSaveData(this);
                }
                Decks[i].Clear();
            }

            if (CardList == null)
            {
                CardList = new CardListSaveData(this);
            }
            CardList.Clear();
        }

        public bool Load()
        {
            return Load(GetSaveFilePath(Version));
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
            Save(GetSaveFilePath(Version));
        }

        public void Save(string path)
        {
            byte[] buffer = ToArray();
            if (buffer != null)
            {
                File.WriteAllBytes(path, buffer);
            }
        }

        public void MigrateFrom(GameSaveData other)
        {
            LotdArchive archive = new LotdArchive(Version);
            archive.Load();
            LotdArchive otherArchive = new LotdArchive(other.Version);
            otherArchive.Load();

            CardList.MigrateFrom(archive, otherArchive, other);
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
                (Stats != null ? Stats : new StatSaveData(this)).Save(writer);

                Debug.Assert(writer.BaseStream.Position == BattlePacksOffset);
                for (int i = 0; i < Constants.NumBattlePacks; i++)
                {
                    (BattlePacks[i] != null ? BattlePacks[i] : new BattlePackSaveData(this)).Save(writer);
                }

                Debug.Assert(writer.BaseStream.Position == MiscDataOffset);
                (Misc != null ? Misc : new MiscSaveData(this)).Save(writer);

                Debug.Assert(writer.BaseStream.Position == CampaignDataOffset);
                (Campaign != null ? Campaign : new CampaignSaveData(this)).Save(writer);

                Debug.Assert(writer.BaseStream.Position == DecksOffset);
                for (int i = 0; i < Constants.NumUserDecks; i++)
                {
                    (Decks[i] != null ? Decks[i] : new DeckSaveData(this)).Save(writer);
                }

                Debug.Assert(writer.BaseStream.Position == CardListOffset);
                (CardList != null ? CardList : new CardListSaveData(this)).Save(writer);

                Debug.Assert(writer.BaseStream.Length == FileLength);

                byte[] buffer = stream.ToArray();
                SaveSignature(buffer);
                return buffer;
            }
        }
    }
}
