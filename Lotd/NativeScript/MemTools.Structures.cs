using Lotd.FileFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Lotd
{
    public partial class MemTools
    {
        [StructLayout(LayoutKind.Sequential)]
        struct DeckEditFilterCards
        {
            public int NumFilteredCards;
            public int NumTotalCards;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.AbsoluteMaxNumCards)]
            public short[] CardIds;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct CardShopOpenPackInfo
        {
            public int NumCards;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public short[] CardIds;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct YdcDeck
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.DeckNameLen)]
            public string DeckName;

            public short NumMainDeckCards;
            public short NumExtraDeckCards;
            public short NumSideDeckCards;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.NumMainDeckCards)]
            public short[] MainDeck;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.NumExtraDeckCards)]
            public short[] ExtraDeck;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.NumSideDeckCards)]
            public short[] SideDeck;

            // These are some kind of unique id
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] Unk1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] Unk2;

            public int Unk3;
            public int Unk4;
            public int Unk5;

            /// <summary>
            /// Avatar / character id of the deck owner
            /// </summary>
            public int DeckAvatarId;

            public int Unk6;
            public int Unk7;

            private int isDeckComplete;// bool

            /// <summary>
            /// Assumed to be used to determine if this deck is complete - should always be 1 for a valid deck
            /// </summary>
            public bool IsDeckComplete
            {
                get { return isDeckComplete != 0; }
                set { isDeckComplete = value ? 1 : 0; }
            }

            public bool IsValid
            {
                get
                {
                    return NumMainDeckCards <= Constants.NumMainDeckCards &&
                           NumExtraDeckCards <= Constants.NumExtraDeckCards &&
                           NumSideDeckCards <= Constants.NumSideDeckCards &&
                           !string.IsNullOrEmpty(DeckName);
                }
            }

            public override string ToString()
            {
                return "deckName: " + DeckName;
            }

            public static YdcDeck Create()
            {
                YdcDeck deck = new YdcDeck();
                deck.MainDeck = new short[Constants.NumMainDeckCards];
                deck.SideDeck = new short[Constants.NumSideDeckCards];
                deck.ExtraDeck = new short[Constants.NumExtraDeckCards];
                deck.Unk1 = new byte[12];
                deck.Unk2 = new byte[12];
                return deck;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CardProps
        {
            private int cardId;//[0]
            private int atk;//[1]
            private int def;//[2]
            private int cardType;//[3]
            private int monsterType;//[4]
            private int attribute;//[5]
            public int Level;//[6]
            private int spellType;//[7]
            public int Unk1;//[8]
            public int PendulumScale1;//[9]
            public int PendulumScale2;//[10]
            public int Unk2;//[11]

            public short CardId
            {
                get { return (short)cardId; }
            }

            public int Atk
            {
                get { return atk * 10; }
                set { atk = value / 10; }
            }

            public int Def
            {
                get { return def * 10; }
                set { def = value / 10; }
            }

            public CardType CardType
            {
                get { return (CardType)cardType; }
                set { cardType = (int)value; }
            }

            public MonsterType MonsterType
            {
                get { return (MonsterType)monsterType; }
                set { monsterType = (int)value; }
            }

            public CardAttribute Attribute
            {
                get { return (CardAttribute)attribute; }
                set { attribute = (int)value; }
            }

            public SpellType SpellType
            {
                get { return (SpellType)spellType; }
                set { spellType = (int)value; }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RawPackDefData
        {
            public int Index;
            public int Series;
            public int Price;
            public int PackType;// 82 (regular) / 66 (battle packs)
            public IntPtr CodeName;// ascii string
            public IntPtr ShopPackData;// utf16 string
            public IntPtr BattlePackData;// utf16 string
            public IntPtr Name;// utf16 string
            public IntPtr UnknownString;// utf16 string
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DuelPlayerInfo
        {
            public int LifePoits;
            public int TotalCardsPlusOne;
            public int TotalCards;
            public int NumCardsInHand;
            public int NumCardsInDeck;
            public int NumCardsInGraveyard;
            public int NumCardsInExtraDeck;
            public int NumCardsBanished;
            public int TurnSummonsActivated;
            public int TurnSummonsCompleted;
            public int TurnTotalCardsSummoned;
            public int TurnSpecialSummons;
            public int UnkInt1;
            public int FieldZoneCardId;
            public int IsPlayerOutOfCards;//bool
            public int UnkInt2;
            public int UnkInt3;
            public int DuelResult;// (0=lose, 1=opponentLifePointsZero, 2=draw, 6=exodia)

            // 0-4 = monster slots
            // 5-9 = spell slots
            // 10 = left pendulum
            // 11 = right pendulum
            // 12 = field spell
            // 13 = ?
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
            public DuelFieldSlotInfo[] Field;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 120)]
            public DuelCardInfo[] Hand;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 120)]
            public DuelCardInfo[] Deck;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 150)]
            public DuelCardInfo[] ExtraDeck;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 150)]
            public DuelCardInfo[] Graveyard;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 225)]// size likely incorrect
            public DuelCardInfo[] Banished;

            public int UnkEndValue1;

            public DuelFieldSlotInfo GetFieldSlot(SlotType slot)
            {
                return Field[(int)slot];
            }
        }

        // TODO: Find card position, it is likely in here somewhere
        [StructLayout(LayoutKind.Explicit, Size = 4)]
        public struct DuelCardInfo
        {
            [FieldOffset(0)]
            private ushort val1;
            [FieldOffset(2)]
            private ushort val2;

            [FieldOffset(0)]
            private int val;

            // val1:
            // xx11111111111111 - cardId (offset:0 bits:14 mask:0x3FFF)
            // x1xxxxxxxxxxxxxx - owner (offset:14 bits:1 mask:1)
            // 1xxxxxxxxxxxxxxx - isXyzSummoned (offset:15 bits:1 mask:1) - assumed to be xyz, could mean something else

            // val2:
            // xxxxxxxxxxxxxxx1 - ? (offset:0 bits:1 mask:1)
            // xxxxxxxxxx11111x - ? (offset:1 bits:5 mask:0x1F)
            // xx11111111xxxxxx - objectId (offset:6 bits:8 mask:0xFF)
            // 11xxxxxxxxxxxxxx - ? (offset:14 bits:2 mask:3)

            // reverse ushort masks used in the client (val2):            
            // 11xxxxxxxx111111 - 0x3FC0 (mask out the objectId)

            // - Find most of these offsets/masks in sub_1400726A0 (set card position func)
            // - This function masks out existing values and sets new ones sub_1408BA400

            public ushort CardId
            {
                get { return (ushort)(val1 & 0x3FFF); }
                set
                {
                    // Mask out the existing card id
                    val1 &= 0xC000;

                    // Set the new card id
                    val1 |= (ushort)(value & 0x3FFF);
                }
            }

            /// <summary>
            /// The player that owns this card
            /// TODO: Check if this actually means "controller" where this defines who currently controls the card
            /// </summary>
            public Player Owner
            {
                get { return (Player)((val1 >> 14) & 1); }
                set
                {
                    // Mask out the existing owner value
                    val1 &= 0xBFFF;

                    // Set the new owner id
                    val1 |= (ushort)((val1 & 1) << 14);
                }
            }

            public bool IsXyzSummoned
            {
                get { return (byte)((val1 >> 15) & 1) != 0; }
            }

            // Likely a flags enum
            public ushort Val2Unk1
            {
                get { return (ushort)(val2 & 0x3F); }
            }

            // Likely a flags enum
            public ushort Val2Unk2
            {
                get { return (ushort)((val2 >> 1) & 0x1F); }
            }

            public ushort Val2Unk3
            {
                get { return (ushort)((val2 >> 14) & 3); }
            }

            public ushort ObjectId
            {
                get { return (ushort)((val2 >> 6) & 0xFF); }
            }
        }

        // TODO: Find cards stacked under an XYZ card
        //
        // XYZ - same card ids stacked
        // 20 a6 c0 0a 01 00 00 01 00 00 05 00 00 00 00 00 10 01 48 00 44 00 00 00 - 2 stack
        // 20 a6 c0 0a 01 00 00 01 09 00 06 00 00 00 00 00 10 01 48 00 44 00 00 00 - 1 stack
        //
        // XYZ - different card ids stacked
        // 20 a6 40 0c 17 00 00 01 00 00 08 00 00 00 00 00 10 01 48 00 44 00 00 00 - 2 stack
        // 20 a6 40 0c 17 00 00 01 08 00 09 00 00 00 00 00 10 01 48 00 44 00 00 00 - 1 stack
        [StructLayout(LayoutKind.Sequential)]
        public struct DuelFieldSlotInfo
        {
            public DuelCardInfo Card;
            public int Unk1;
            public int Unk2;
            public int Unk3;
            public int Unk4;
            public int Unk5;
        }

        public enum SlotType
        {
            Monster1 = 0,
            Monster2 = 1,
            Monster3 = 2,
            Monster4 = 3,
            Monster5 = 4,

            Spell1 = 5,
            Spell2 = 6,
            Spell3 = 7,
            Spell4 = 8,
            Spell5 = 9,

            LeftPendulum = 10,
            RightPendulum = 11,

            FieldSpell = 12,

            Hand = 13,
            ExtraDeck = 14,
            Deck = 15,
            Graveyard = 16,
            Banished = 17
        }

        public enum AudioSnippet
        {
            system_deck = 0,
            system_result = 1,
            duel_normal_2_t = 2,

            //unused = 3,

            duel_1_r = 4,
            duel_2_y = 5,
            duel_2_r = 6,
            mus_vs01 = 7,
            mus_tutorial = 8,
            mus_title = 9,
            mus_duel_03 = 10,
            mus_duel_01 = 11,
            cursor = 12,
            decide = 13,
            cancel = 14,
            unable = 15,
            menu = 16,
            page = 17,
            info = 18,
            tab = 19,
            card_araware = 20,
            card_crash = 21,
            card_oki = 22,
            disk_card_in = 23,
            field_card_up = 24,
            field_mon_deru = 25,
            turnex = 26,
            phlogo = 27,
            card_move_1 = 28,
            card_move_3 = 29,
            card_move_4 = 30,
            arrow = 31,
            chain = 32,
            num_red = 33,
            num_green = 34,
            fusion = 35,
            coin = 36,
            field_change = 37,
            duel_end = 38,
            card_disappear = 39,
            spark = 40,
            dice_1 = 41,
            dice_2 = 42,
            dice_3 = 43,
            dice_4 = 44,
            arcana_1 = 45,
            arcana_2 = 46,
            d_draw = 47,
            rq_fx1 = 48,
            rq_fx2 = 49
        }

        /// <summary>
        /// Custom structure holding info about screen state transitions
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        struct ScreenStateInfo
        {
            public int State;
            public int TransitionType;
            public double TransitionTime;
        }

        // This is probably flags but cant get it to behave properly with
        // certain flags on their own. Putting constant values so there isn't confusion
        // as to why some things don't work on their own.
        public enum ScreenTransitionType
        {
            Default = BlackFadeOutFadeIn,

            WhiteFadeOut = 8,//0000 0000 0000 1000 - bits
            BlackFadeOut = 257,//0000 0001 0000 0001 - bits

            WhiteFadeOutFadeIn = 272,//0000 0001 0001 0000 - bits
            BlackFadeOutFadeIn = 273,//0000 0001 0001 0001 - bits
        }

        public enum ScreenState
        {
            /// <summary>
            /// Second logo that appears
            /// </summary>
            DeveloperLogo = 2,

            /// <summary>
            /// First logo that appears
            /// </summary>
            PublisherLogo = 4,

            /// <summary>
            /// Third logo that appears ("Press Any Button")
            /// </summary>
            GameLogo = 5,

            // 6 = clickable game logo?
            // 7 = transition between logo / main menu?

            MainMenu = 8,

            /// <summary>
            /// Loading screen that is displayed before a duel
            /// </summary>
            DuelLoadingScreen = 9,

            // SeriesSelection = 10,

            /// <summary>
            /// Duel state after loading screen (this includes rock paper scissors)
            /// </summary>
            Duel = 11,

            HelpAndOptions = 12,
            Settings = 13,
            VideoSettings = 14,
            Credits = 15,
            Controls = 16,
            HowToPlay = 17,

            /// <summary>
            /// Hidden "Statistics" menu
            /// </summary>
            Statistics = 18,

            /// <summary>
            /// The list of players when in multiplayer mode
            /// </summary>
            PlayersList = 19,

            DuelistChallenges = 21,

            /// <summary>
            /// Dialog that happens before / after a duel
            /// </summary>
            CampaignDialog = 22,

            /// <summary>
            /// "Choose Deck" for campaign mode duels (Story Deck / User Deck)
            /// </summary>
            DeckSelection = 23,

            Tutorial = 24,
            DeckEdit = 25,

            /// <summary>
            /// Deck editor during a match duel by using the side deck
            /// </summary>
            MatchDuelEditDeck = 26,

            /// <summary>
            /// Match duel round results / "Interim Results"
            /// </summary>
            MatchDuelRoundResults = 27,

            /// <summary>
            /// Duel result screen that is displayed once the duel is finished
            /// </summary>
            DuelResult = 28,

            CardShop = 29,

            /// <summary>
            /// Battle pack main menu
            /// </summary>
            BattlePack = 30,

            /// <summary>
            /// Unwrapping battle pack in battle pack draft
            /// </summary>
            BattlePackDraft = 31,

            /// <summary>
            /// Edit battle pack deck menu
            /// </summary>
            BattlePackEdit = 32,

            /// <summary>
            /// "Find Match" / "Create Match" menu in multiplayer
            /// </summary>
            MultiplayerPlayerMatch = 33,

            /// <summary>
            /// "Create Match" menu in multiplayer
            /// </summary>
            MultiplayerCreateMatch = 34,

            MultiplayerLobby = 35,

            MultiplayerLobby2 = 36,

            // 37 = multiplayer duel?

            MultiplayerLeaderboars = 38,

            // "Join" menu in multiplayer
            MultiplayerJoin = 39,

            /// <summary>
            /// "Select Series" menu
            /// </summary>
            SeriesSelection = 41,

            /// <summary>
            /// "Select Duel" menu for campaign mode
            /// </summary>
            CampaignDuelSelection = 42,

            /// <summary>
            /// "Score Review" menu
            /// </summary>
            ScoreReview = 43
        }

        /// <summary>
        /// The dueling arena / background environment the duel will be played on
        /// </summary>
        public enum DuelArena
        {
            Default = 0,
            DuelistKingdom = 1,
            YuGiOh5D = 2,
            YuGiOhZEXAL = 3,
            YuGiOhGX = 4,
            YuGiOhARCV = 5,
            BattleCityBlimp = 6,
            BattleCity = 7,
            ZexalBarian = 8
        }

        /// <summary>
        /// Holds information about starting a duel
        /// </summary>
        public class StartDuelInfo
        {
            /// <summary>
            /// Changes the screen state to main menu before loading the duel. This is to fully reload the state.
            /// </summary>
            public bool FullReload { get; set; }

            /// <summary>
            /// Skips the rock paper scissors before the duel
            /// </summary>
            public bool SkipRockPaperScissors { get; set; }

            /// <summary>
            /// 3 field slots instead of 5, 4 starting cards instead of 5.
            /// </summary>
            public bool SpeedDuel { get; set; }

            /// <summary>
            /// Rush duel rules (3 field slots, start every turn with 5 cards, etc)
            /// </summary>
            public bool RushDuel { get; set; }

            /// <summary>
            /// If true this will use use 4000 life points instead of 8000 (SpeedDuel must be true)
            /// </summary>
            public bool UseSpeedDuelLifePoints { get; set; }

            /// <summary>
            /// "Tag duel" mode where there are 4 players
            /// </summary>
            public bool TagDuel { get; set; }

            /// <summary>
            /// If true this will be a "match" duel (3 rounds)
            /// </summary>
            public bool Match { get; set; }

            /// <summary>
            /// Used for internal testing? Or not fully understanding the use of this?
            /// </summary>
            public TestDuelOption TestOption { get; set; }

            public DuelArena Arena { get; set; }
            public Player StartingPlayer { get; set; }

            public int[] DeckIds { get; private set; }
            public int[] AvatarIds { get; private set; }

            /// <summary>
            /// Defines who controls which players (SkipRockPaperScissors must be true)
            /// </summary>
            public PlayerController[] Controllers { get; private set; }

            /// <summary>
            /// Defines the AI mode (SkipRockPaperScissors must be false)
            /// </summary>
            public AIMode AIMode { get; set; }

            /// <summary>
            /// The type of duel (campaign, battle pack, challenge, tutorial)
            /// </summary>
            public DuelType DuelType { get; set; }

            /// <summary>
            /// The tutorial duel index to play (e.g. 16 = tagDuel, 24 = last index (pendulum), 25+ = crash)
            /// </summary>
            public int TutorialDuelIndex { get; set; }

            /// <summary>
            /// The index of the campaign duel (see dueldata_X.bin)
            /// </summary>
            public int CampaignDuelIndex { get; set; }

            /// <summary>
            /// The campaign duel user deck index (use -1 for the story deck)
            /// </summary>
            public int CampaignDuelDeckIndex { get; set; }

            /// <summary>
            /// The index of the battle pack to use for the duel (0-4) (draft/sealed epic dawn, war of the giants, etc)
            /// </summary>
            public int BattlePackIndex { get; set; }

            /// <summary>
            /// How many life points each player starts with
            /// </summary>
            public int LifePoints { get; set; }

            /// <summary>
            /// Enables a time limit for each turn
            /// Note: The timer wont change for the second player / AI
            /// </summary>
            public bool TurnTimeLimitEnabled { get; set; }

            /// <summary>
            /// The time limit for each turn (anything above 0 will show a timer)
            /// Note: The timer wont change for the second player / AI
            /// </summary>
            public long TurnTimeLimit { get; set; }

            /// <summary>
            /// The number of cards in each players starting hand (-1 for default)
            /// </summary>
            public int StartingHandCount { get; set; }

            /// <summary>
            /// Seeds the random number generator used for various things including randomizing the deck (0 = random seed)
            /// </summary>
            public int RandSeed { get; set; }

            /// <summary>
            /// If true use master rules 5 (2020 rules / link evolution v2)
            /// </summary>
            public bool MasterRules5 { get; set; }

            public StartDuelInfo()
            {
                DeckIds = new int[Constants.MaxNumPlayers];
                AvatarIds = new int[Constants.MaxNumPlayers];
                Controllers = new PlayerController[Constants.MaxNumPlayers];

                SetController(Player.Self, PlayerController.Player);
                SetController(Player.Opponent, PlayerController.AI);
                SetController(Player.TagSelf, PlayerController.Player);
                SetController(Player.TagOpponent, PlayerController.AI);

                LifePoints = Constants.DefaultLifePoints;
                UseSpeedDuelLifePoints = true;
                StartingHandCount = -1;
                CampaignDuelDeckIndex = -1;
                MasterRules5 = true;
            }

            public void SetDeckId(Player player, int deckId)
            {
                DeckIds[(int)player] = deckId;
            }

            public void SetAvatarId(Player player, int avatarId)
            {
                AvatarIds[(int)player] = avatarId;
            }

            public void SetController(Player player, PlayerController controller)
            {
                Controllers[(int)player] = controller;
            }

            public int GetDeckId(Player player)
            {
                return DeckIds[(int)player];
            }

            public int GetAvatarId(Player player)
            {
                return AvatarIds[(int)player];
            }

            public PlayerController GetController(Player player)
            {
                return Controllers[(int)player];
            }
        }

        public enum DuelType
        {
            None,
            Campaign,
            BattlePack,
            Challenge,
            Tutorial,
        }

        // Duel links calls thie "LimitedType"
        public enum TestDuelOption
        {
            //1 = disable normal summoning monsters
            //2 = disable special summoning monsters? (assumed based on a Duel Links enum)
            //3 = disable the ability to "set" cards
            //4 = disable tribute summon? (assumed based on a Duel Links enum)
            //5 = disable attack? (assumed based on a Duel Links enum)
            //6 = no hand? (assumed based on a Duel Links enum)
            //7 = draw 2x the number of cards? (assumed based on a Duel Links enum)
            //8 = activates "Final Countdown" at the start of the duel
            //9 = both players take 500 damage at the start of each turn
            //10 = activates AIvsAI mode after the first input?
            //11 = disables phase menu
            //12 = ? (Vs2on1 - duel links)
            //13 = opponent draws double the number of cards (first draw only) (Vs2on1_Hand - duel links)

            None,
            DisableNormalSummon = 1,
            DisableCardSet = 3,
            FinalCountdown = 8,
            TurnStart500Dmg = 9,
            AIVsAI = 10,
            DisablePhaseMenu = 11,
            OpponentFirsDrawDoubleCards = 13
        }

        /// <summary>
        /// The AI mode
        /// </summary>
        public enum AIMode
        {
            /// <summary>
            /// Player vs AI (the default AI mode)
            /// </summary>
            Default = PlayerVsAI,

            /// <summary>
            /// Player vs AI (the default AI mode)
            /// </summary>
            PlayerVsAI = 0,
            /// <summary>
            /// Player vs AI reversed (this is different from a reverse duel as your field slots appear red)
            /// </summary>
            PlayerVsAIReverse = 1,

            /// <summary>
            /// AI vs AI
            /// </summary>
            AIVsAI = 2,
            /// <summary>
            /// AI vs AI reversed
            /// </summary>
            AIVsAIReverse = 3,

            // 4+ will crash the client once the duel is complete
        }

        /// <summary>
        /// The controller for a player (player / AI)
        /// </summary>
        public enum PlayerController
        {
            Player = 0,
            AI = 1,
            // 2 = no cards show, both players have 0 life points, phase menu doesnt work
            // 3 = you control the opponent but your cards are always back-facing for the 2nd player
            // 4 = cards show but cant do any actions and phase menu doesnt work
            // 5+ = same as 3
        }

        public enum Player
        {
            Self,
            Opponent,

            // Generally avoid using these values for most things other than initialization as
            // most code works on a single player/opponent 
            TagSelf,
            TagOpponent
        }
    }
}
