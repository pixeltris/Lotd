using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lotd
{
    public static class Constants
    {
        public const GameVersion LatestVersion = GameVersion.LinkEvolution2;
        public const int AbsoluteMaxNumCards = 20000;// Keep this as a constant

        public static int GetNumCards(GameVersion version)
        {
            switch (version)
            {
                default:
                case GameVersion.Lotd:
                    return 7581;
                case GameVersion.LinkEvolution1:
                case GameVersion.LinkEvolution2:
                    return 10166;
            }
        }

        public static int GetNumCards2(GameVersion version)
        {
            switch (version)
            {
                default:
                case GameVersion.Lotd:
                    return GetNumCards(version);
                case GameVersion.LinkEvolution1:
                case GameVersion.LinkEvolution2:
                    return 20000;
            }
        }
        public static ushort GetMaxCardId(GameVersion version)
        {
            switch (version)
            {
                default:
                case GameVersion.Lotd:
                    return 12432;
                case GameVersion.LinkEvolution1:
                case GameVersion.LinkEvolution2:
                    return 14969;
            }
        }

        /// <summary>
        /// Number of duel series (YuGiOh, GX, 5D, ZEXAL, ARCV)
        /// </summary>
        public static int GetNumDuelSeries(GameVersion version)
        {
            switch (version)
            {
                default:
                case GameVersion.Lotd:
                    return 5;
                case GameVersion.LinkEvolution1:
                case GameVersion.LinkEvolution2:
                    return 6;
            }
        }

        /// <summary>
        /// Number of available user deck slots which can be created in in the deck editor
        /// </summary>
        public const int NumUserDecks = 32;

        /// <summary>
        /// Number of available battle packs (all sealed packs + all draft packs)
        /// </summary>
        public const int NumBattlePacks = 5;

        /// <summary>
        /// Number of deck slots available which map into deckdata_X.bin
        /// </summary>
        public static int GetNumDeckDataSlots(GameVersion version)
        {
            switch (version)
            {
                default:
                case GameVersion.Lotd:
                    return 477;
                case GameVersion.LinkEvolution1:
                case GameVersion.LinkEvolution2:
                    return 700;
            }
        }

        /// <summary>
        /// The length of a deck name (this is technically 32 with a null terminator)
        /// </summary>
        public const int DeckNameLen = 33;

        /// <summary>
        /// Number of usable characters in a deck name (1 less than DeckNameLen as 1 is reversed for null terminator)
        /// </summary>
        public const int DeckNameUsableLen = 32;

        /// <summary>
        /// The length of a deck name in bytes
        /// </summary>
        public const int DeckNameByteLen = DeckNameLen * 2;

        /// <summary>
        /// Number of slots available in data for main deck cards
        /// </summary>
        public const int NumMainDeckCards = 60;

        /// <summary>
        /// Number of slots available in data for side deck cards
        /// </summary>
        public const int NumSideDeckCards = 15;

        /// <summary>
        /// Number of slots available in data for extra deck cards
        /// </summary>
        public const int NumExtraDeckCards = 15;

        public const int NumMinMainDeckCards = 40;

        public const int NumMinMainDeckCardsSpeedDuel = 20;
        public const int NumMainDeckCardsSpeedDuel = 30;
        public const int NumExtraDeckCardsSpeedDuel = 5;
        public const int NumSideDeckCardsSpeedDuel = 5;

        /// <summary>
        /// The starting deck index for user deck indexes in memory (32 user decks)
        /// </summary>
        public const int DeckIndexUserStart = 0;
        /// <summary>
        /// The starting deck index for data/ydc deck indexes in memory (477 ydc decks) (note that index 0 is an empty entry)
        /// </summary>
        public const int DeckIndexYdcStart = 32;

        /// <summary>
        /// Max number of players there can be (4 defined by tag duel)
        /// </summary>
        public const int MaxNumPlayers = 4;

        /// <summary>
        /// The default life points for each player (8000)
        /// </summary>
        public const int DefaultLifePoints = 8000;
    }
}
