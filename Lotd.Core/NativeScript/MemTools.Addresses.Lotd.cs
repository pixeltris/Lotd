using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotd
{
    public partial class MemTools
    {
        class Addresses_Lotd
        {
            // The in-memory version of various structures loaded from files in the archive - see memory.c for more info
            // - These can be found quite easily by strings "main/chardata_#.bin", ".ydc", "main/deckdata_#.bin", etc
            public IntPtr ydcDecksAddress = (IntPtr)0x1410AB3B0;// ydc decks (battle packs are loaded dynamically and don't use the in-memory version)
            public IntPtr chardataBinAddress = (IntPtr)0x140E92A50;// chardata_X.bin (character data)
            public IntPtr dueldataBinAddress = (IntPtr)0x140E9AA10;// dueldata_X.bin (campaign duel data)
            public IntPtr deckdataBinAddress = (IntPtr)0x140E8B300;// dueldata_X.bin (links the ydc files)
            public IntPtr cardPropsBinAddress = (IntPtr)0x141019880;// CARD_Prop.bin (atk/def/level/etc) - see CARD_Prop.c (sub_1408975D0)
            public IntPtr packDefDataBinAddress = (IntPtr)0x140EA0540;// packdefdata_X.bin packdata_xxx.bin / bpack_xxx.bin (shop packs / battle packs)

            // This holds onto the dataLength(int64)/dataPointer(int64) array for CARD_XXX files
            public IntPtr cardDataListStartAddress = (IntPtr)0x140F834E0;
            public IntPtr cardGenreBinAddress = (IntPtr)0x140F83560;// CARD_Genre.bin (address=dataLen, address+8=dataPtr)

            // Address of the minimum duration in seconds the duel loading screen will wait (this is a double)
            // See screenstate.c for more info on the function which handles the duel loading screen (sub_140696E20)
            public IntPtr duelLoadingScreenDelayAddress = (IntPtr)0x140ACD828;

            // A core gameplay structure holding many things - see loadduel_addresses.c
            public IntPtr coreDuelDataAddress = (IntPtr)0x1410CF430;

            // The duel info for each player (life points, hand, deck, field, etc) - see YGOfuncThreadDuel.c
            public IntPtr duelPlayerInfoAddress = (IntPtr)0x14117D580;

            // Player turn / turn count. This can be found in action id 0x4 func sub_1404937C0 - see YGOfuncThreadDuel.c / YGOPopcornCore_beginduel_setDuelInfo.c
            public IntPtr playerTurnAddress = (IntPtr)(0x141180ADC + 4);// +4 holds a more correct value (dword_141180AE0)
            public IntPtr turnCountAddress = (IntPtr)0x141180AE8;

            // Various "modes" which you can set when starting a duel (see YGOPopcornCore_beginduel_setDuelInfo.c)
            public IntPtr modeMatchDuelAddress = (IntPtr)0x140D99D6C;// Duel is a match (3 rounds)
            public IntPtr modeTagDuelAddress = (IntPtr)0x140D99D6D;// Tag duel (4 players)
            public IntPtr modeTestOptionAddress = (IntPtr)0x140D99D70;// Used for tests? (disable normal summons and some other things)
            public IntPtr modeNetworkedGameAddress = (IntPtr)0x140D99BF6;// An online / networked game
            public IntPtr modeNetworkedGameIsCreatorAddress = (IntPtr)0x140D99BF7;// Creator of the networked game ("Create" instead of "Find")
            public IntPtr modeNetworkedGameRankedAddress = (IntPtr)0x140D99BF8;// Ranked networked game
            public IntPtr modeInstantStartDuelAddress = (IntPtr)0x140D99BF9;// Instantly starts the duel / skips rock paper scissors
            public IntPtr modePlayerControllerAddress = (IntPtr)0x140D99C28;// Who controls which player (AI/player) (0-3 values) (requires modeInstantStartDuelAddress=true)
            public IntPtr modeAIModeAddress = (IntPtr)0x140D99DA0;// The AI mode (playerVsAI / AIVsAI etc) (requires modeInstantStartDuelAddress=false)
            public IntPtr modeStartingLifePointsAddress = (IntPtr)0x140D99D80;// How many life points each player starts with
            public IntPtr modeStartingPlayerAddress = (IntPtr)0x140D99D94;// Player who will start first
            public IntPtr modeTurnTimeLimitEnabledAddress = (IntPtr)0x140D99D74;// Turn time limit enabled state
            public IntPtr modeTurnTimeLimitAddress = (IntPtr)0x140D99D78;// Turn time limit (anything abouve 0 will show a timer)
            public IntPtr modeTutorialDuelAddress = (IntPtr)0x140D99BFA;// This is a tutorial duel
            public IntPtr modeTutorialDuelIndexAddress = (IntPtr)0x140D99BFC;// The tutorial duel index
            public IntPtr modeCampaignDuelAddress = (IntPtr)0x140D99BE8;// The duel is a campaign duel        
            public IntPtr modeCampaignDuelIndexAddress = (IntPtr)0x140D99BF0;// The campaign duel index
            public IntPtr modeCampaignDuelDeckIndexAddress = (IntPtr)0x140D99C04;// The user deck index for the campaign duel (-1 = story deck)
            public IntPtr modeBattlePackDuelAddress = (IntPtr)0x140D99BE0;// The duel is a battle pack duel
            public IntPtr modeBattlePackIndexAddress = (IntPtr)0x140D99BE4;// The index of the battle pack (0-4) (draft/sealed epic dawn, war of the giants, etc)
            public IntPtr modeChallengeDuelAddress = (IntPtr)0x140D99BF5;// The Duel is a challenge duel
            public IntPtr modeMasterRules = (IntPtr)0;// 1=2020 master rules, 0=2019 (this is LE only)

            // The default hand size limit (this is an ASM instruction / code address) - see discard_cards.c
            public IntPtr handSizeLimitAddress = (IntPtr)0x14006F8E1;

            // "YGO:PopcornCore" data address - see main_data_address.c
            public IntPtr popcornCoreDataAddress = (IntPtr)0x140EA2148;

            // Holds the duel arena id for the current duel (at load) - see YGOWorkerThread_arena.c
            public IntPtr duelArenaAddress = (IntPtr)0x140D99C00;

            // Holds the avatar id for each player for the current duel (at load) - see character_avatars.c / YGOWorkerThread_loadduel.c
            public IntPtr playerAvatarIdAddress = (IntPtr)0x140D99C08;

            // Holds the deck id for each player for the current duel (at load) - see YGOWorkerThread_loadduel.c
            public IntPtr playerDeckIdAddress = (IntPtr)0x140D99C18;

            // Save data addresses. Not entirely sure on their actual names / uses - see savegame.c
            // - 140E9EF00 seems to be some global which is used with save data functions?
            // - 140E9F048 holds an interface to steam related save data? (if you follow this address downward twice you see steam strings)
            public IntPtr saveDataAddress1 = (IntPtr)0x140E9EF00;
            public IntPtr saveDataAddress2 = (IntPtr)0x140E9F048;

            // Default structure decks list used to give the player the default cards (deckEditCardCount.c)
            public IntPtr defaultStructureDeckCardsAddress = (IntPtr)0x140AC0170;

            // Address of the card shop info - see cardShopOpenPack.c
            public IntPtr cardShopAddress = (IntPtr)0x140EA21A0;

            // Time related addresses (the pointers to the API functions - we will replace them instead of hooking the API itself)
            public IntPtr queryPerformanceCounterAddress = (IntPtr)0x140A62078;
            public IntPtr getTickCount64Address = (IntPtr)0x140A62130;
            public IntPtr getTickCountAddress = (IntPtr)0;
            public IntPtr timeGetTimeAddress = (IntPtr)0x140A624A8;

            // LE+ this address points to a jnz instruction which we will change to a jmp - see pauseOnWindowFocusLost_LE_v2.c
            public IntPtr windowFocusPauseAddress = (IntPtr)0;

            // Offsets

            public int saveDataMemOffset = 88;// See below
            public int saveDataAddressOffset = 7624;// See sub_1406551F0
            public int popcornCoreOffset = 472;// See sub_1406341D0
            public int deckEditReturnScreenStateOffset = 44;// See sub_1406939D0
            public int deckEditBaseOffset = 608;
            public int deckEditTrunkOffset = 9040 + 728;// See sub_1406850F0
            public int deckEditUserDecksOffset = 5424 + 376 + 232;// See sub_140680A80 / sub_14065E990
            public int deckEditFilteredCardCountOffset = 500;
            public int deckEditCardCountOffset = 504;
            public int deckEditCardsOffset = 512;
            public int cardShopRevealedCardIdsOffset = 4280;// See sub_1406506E0
            public int cardShopIsOpeningPackOffset = 4272;// See sub_1406529C0
            public int dirtySaveDataOffset1 = 29068;// See sub_14061FAF0
            public int dirtySaveDataOffset2 = 29064;// See sub_14061FAF0
            public int dirtySaveDataOffset3 = 29232;// See sub_14061FAF0
            public int screenStateOffset1 = 888;// See sub_140808640
            public int screenStateOffset2 = 884;// See sub_140808640
            public int screenStateOffset3 = 48;// See sub_140808640
            public int hoveredCardOffset1 = 252;// See sub_14089BFC0
            public int hoveredCardOffset2 = 9176;// See sub_140876690
            public int hoveredCardOffset3 = 6384;// See sub_140876690
            public int hoveredCardOffset4 = 48;// See sub_14089BFC0

            public MemTools Mem;

            public IntPtr GetBaseSaveDataAddress()
            {
                ///////////////////////////////////////
                // begin sub_14061A590 (sub_1408004D0 in LEv2)
                ///////////////////////////////////////

                int v2 = -3;
                if (v2 == -3)
                {
                    v2 = Mem.ReadValue<int>(saveDataAddress1 + 176);
                }

                IntPtr v3 = Mem.ReadValue<IntPtr>(saveDataAddress1);
                IntPtr v4 = Mem.ReadValue<IntPtr>(v3 + 8);
                IntPtr v5 = v3;
                while (Mem.ReadValue<byte>(v4 + 25) == 0 && Mem.HasProcessHandle)
                {
                    if (Mem.ReadValue<int>(v4 + 32) >= v2)
                    {
                        v5 = v4;
                        v4 = Mem.ReadValue<IntPtr>(v4);
                    }
                    else
                    {
                        v4 = Mem.ReadValue<IntPtr>(v4 + 16);
                    }
                }
                if (v5 == v3 || v2 < Mem.ReadValue<int>(v5 + 32))
                {
                    v5 = v3;
                }

                // The client does a check for v5 == v3, skipping that check as it has a lot of code
                // and is possibly related to initialization if something isnt initialized yet
                IntPtr result = Mem.ReadValue<IntPtr>(v5 + 40);

                ///////////////////////////////////////
                // end sub_14061A590
                ///////////////////////////////////////

                if (result == IntPtr.Zero)
                {
                    return IntPtr.Zero;
                }

                // Now we have the an address which we can use to get the in-memory version of save data

                // - There are two versions? one at 88 and one at 80?
                // - The one at +80 seems to be used when saving
                // - The one at +88 seems to be used all other times (+88 possibly copies to +80 on save)            

                // The actual start of the data which appears in the save file is at +20
                //IntPtr saveSaveDataAddress = ReadValue<IntPtr>(result + 80);
                //return saveSaveDataAddress + 20;

                IntPtr saveDataAddress = Mem.ReadValue<IntPtr>(result + saveDataMemOffset);
                return saveDataAddress;
            }

            public IntPtr GetSaveDataAddress()
            {
                IntPtr saveDataAddress = GetBaseSaveDataAddress();
                return Mem.ReadValue<IntPtr>(saveDataAddress + saveDataAddressOffset);
            }

            /// <summary>
            /// This holds the default cards list used to display the card list in the deck editor
            /// - Will be empty if you don't open the deck editor
            /// </summary>
            public IntPtr GetDeckEditorOwnedCardListAddress()
            {
                IntPtr saveDataAddress = GetBaseSaveDataAddress();
                if (saveDataAddress == IntPtr.Zero)
                {
                    return IntPtr.Zero;
                }
                return saveDataAddress + 40;// Might be different in LE?
            }
        }
    }
}
