using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd
{
    public class MiscSaveData : SaveDataChunk
    {
        public int DuelPoints { get; set; }

        /// <summary>
        /// This holds an array size of 477 challanges which maps to all available deck ids in deckdata.bin
        /// To set a challenge to complete get the deck id for a challenge and use the id as an index into this array.
        /// For example to set challenge state using CharData use Challenges[charData.ChallengeDeckId] = DeulistChallengeState.XXXX;
        /// </summary>
        public DeulistChallengeState[] Challenges { get; private set; }

        /// <summary>
        /// Each recipe is represented by a single bit in the save file. The recipe index is the id defined in deckdata.bin
        /// There should be 477 available slots. To set a recipe to unlocked find the deck id and use the id as an index into this array.
        /// For example UnlockedRecipes[deckData.Id1] = true;
        /// </summary>
        public bool[] UnlockedRecipes { get; set; }

        /// <summary>
        /// Each avatar is represented by a single bit in the save file.
        /// Use the character/avatar index in chardata.bin to index into this array.
        /// </summary>
        public bool[] UnlockedAvatars { get; set; }

        public CompleteTutorials CompleteTutorials { get; set; }
        public UnlockedContent UnlockedContent { get; set; }
        public UnlockedShopPacks UnlockedShopPacks { get; set; }
        public UnlockedBattlePacks UnlockedBattlePacks { get; set; }

        public MiscSaveData()
        {
            Challenges = new DeulistChallengeState[Constants.NumDeckDataSlots];
            UnlockedRecipes = new bool[Constants.NumDeckDataSlots];
            UnlockedAvatars = new bool[153];
        }

        public override void Clear()
        {
            // Should we clear this to 0 or default points (1000)?
            DuelPoints = 1000;

            for (int i = 0; i < Challenges.Length; i++)
            {
                Challenges[i] = DeulistChallengeState.Locked;
            }

            for (int i = 0; i < UnlockedRecipes.Length; i++)
            {
                UnlockedRecipes[i] = false;
            }

            CompleteTutorials = CompleteTutorials.None;
            UnlockedContent = UnlockedContent.None;
            UnlockedShopPacks = UnlockedShopPacks.None;
            UnlockedBattlePacks = UnlockedBattlePacks.None;
        }

        public override void Load(BinaryReader reader)
        {
            // 64 starting bytes
            reader.ReadBytes(16);

            // DP is treated as a long in save data
            DuelPoints = (int)reader.ReadInt64();

            byte[] unlockedAvatarsBuffer = reader.ReadBytes(32);
            for (int i = 0; i < UnlockedAvatars.Length; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;
                UnlockedAvatars[i] = (unlockedAvatarsBuffer[byteIndex] & (byte)(1 << bitIndex)) != 0;
            }

            for (int i = 0; i < Constants.NumDeckDataSlots; i++)
            {
                Challenges[i] = (DeulistChallengeState)reader.ReadInt32();
            }
            
            byte[] unlockedRecipesBuffer = reader.ReadBytes(60);
            for (int i = 0; i < Constants.NumDeckDataSlots; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;
                UnlockedRecipes[i] = (unlockedRecipesBuffer[byteIndex] & (byte)(1 << bitIndex)) != 0;
            }
            
            UnlockedShopPacks = (UnlockedShopPacks)reader.ReadUInt32();
            UnlockedBattlePacks = (UnlockedBattlePacks)reader.ReadUInt32();
            reader.ReadBytes(8);

            CompleteTutorials = (CompleteTutorials)reader.ReadInt32();
            UnlockedContent = (UnlockedContent)reader.ReadInt32();
        }

        public override void Save(BinaryWriter writer)
        {
            writer.Write(new byte[16]);/*
            {
                0x20, 0x83, 0xA1, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x83, 0xA1, 0xFF
            });*/

            // DP is treated as a long in save data
            writer.Write((long)DuelPoints);

            // Avatar data (you can assign these avatars as your avatar for your deck)
            // This might be less than 32 bytes, ids only go up to 152 in chardata.bin (19/20 bytes total)
            byte[] unlockedAvatarsBuffer = new byte[32];
            for (int i = 0; i < UnlockedAvatars.Length; i++)
            {
                if (UnlockedAvatars[i])
                {
                    int byteIndex = i / 8;
                    int bitIndex = i % 8;
                    unlockedAvatarsBuffer[byteIndex] |= (byte)(1 << bitIndex);
                }
            }
            writer.Write(unlockedAvatarsBuffer);

            // Challenge data
            for (int i = 0; i < Constants.NumDeckDataSlots; i++)
            {
                writer.Write((int)Challenges[i]);
            }

            // Unlocked recipes
            byte[] unlockedRecipesBuffer = new byte[60];
            for (int i = 0; i < UnlockedRecipes.Length; i++)
            {
                if (UnlockedRecipes[i])
                {
                    int byteIndex = i / 8;
                    int bitIndex = i % 8;
                    unlockedRecipesBuffer[byteIndex] |= (byte)(1 << bitIndex);
                }
            }
            writer.Write(unlockedRecipesBuffer);

            writer.Write((uint)UnlockedShopPacks);
            writer.Write((uint)UnlockedBattlePacks);
            writer.Write(new byte[8]);/*
                {
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                });*/

            writer.Write((int)CompleteTutorials);
            writer.Write((int)UnlockedContent);
        }
    }

    public enum DeulistChallengeState
    {
        /// <summary>
        /// Challenge hasn't been unlocked
        /// </summary>
        Locked = 0,

        /// <summary>
        /// Challenge is available and not yet attempted (has '!' mark)
        /// </summary>
        Available = 1,

        /// <summary>
        /// Available, attempted but failed to complete
        /// </summary>
        Failed = 2,

        /// <summary>
        /// ompleted
        /// </summary>
        Complete = 3
    }

    [Flags]
    public enum UnlockedShopPacks : uint
    {
        None = 0,
        //Skipped = 1 << 0

        GrandpaMuto = 1 << 1,
        MaiValentine = 1 << 2,
        Bakura = 1 << 3,
        JoeyWheeler = 1 << 4,
        SetoKaiba = 1 << 5,
        Yugi = 1 << 6,
        AlexisRhodes = 1 << 7,
        BastionMisawa = 1 << 8,
        ChazzPrinceton = 1 << 9,
        SyrusTruesdale = 1 << 10,
        JesseAnderson = 1 << 11,
        JadenYuki = 1 << 12,
        TetsuTrudge = 1 << 13,
        LeoLuna = 1 << 14,
        AkizaIzinski = 1 << 15,
        JackAtlas = 1 << 16,
        Crow = 1 << 17,
        YuseiFudo = 1 << 18,
        CathyKatherine = 1 << 19,
        Quinton = 1 << 20,
        KiteTenjo = 1 << 21,
        Shark = 1 << 22,
        YumaTsukumo = 1 << 23,

        // Any of the following values will unlock all ARC-V packs.
        Pendulum = 1 << 24,
        GongStrong = 1 << 25,
        ZuzuBoyle = 1 << 26,

        All = 0xFFFFFFFF
    }

    [Flags]
    public enum UnlockedBattlePacks
    {
        None = 0,
        WarOfTheGiants = 1 << 0,//1
        WarOfTheGiantsRound2 = 1 << 1,//2
        All = WarOfTheGiants | WarOfTheGiantsRound2//3
    }

    [Flags]
    public enum UnlockedContent
    {
        None = 0,
        DuelistChallenges = 1 << 0,//1
        BattlePack = 1 << 1,//2
        CardShop = 1 << 2,//4
        All = DuelistChallenges | BattlePack | CardShop//7
    }

    [Flags]
    public enum CompleteTutorials
    {
        None = 0,
        //Skipped = 1 << 0,
        Tut01 = 1 << 1,
        Tut02 = 1 << 2,
        Tut03 = 1 << 3,
        Tut04 = 1 << 4,
        Tut05 = 1 << 5,
        Tut06 = 1 << 6,
        Tut07 = 1 << 7,
        Tut08 = 1 << 8,
        Tut09 = 1 << 9,
        Tut10 = 1 << 10,
        Tut11 = 1 << 11,
        Tut12 = 1 << 12,
        Tut13 = 1 << 13,
        Tut14 = 1 << 14,
        Tut15 = 1 << 15,
        //Skipped = 1 << 16,
        Tut16 = 1 << 17,
        Tut17 = 1 << 18,
        Tut18 = 1 << 19,
        Tut19 = 1 << 20,
        Tut20 = 1 << 21,
    }
}
