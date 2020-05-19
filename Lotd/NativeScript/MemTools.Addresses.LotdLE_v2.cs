using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotd
{
    public partial class MemTools
    {
        class Addresses_LotdLE_v2 : Addresses_Lotd
        {
            public Addresses_LotdLE_v2()
            {
                ydcDecksAddress = (IntPtr)0x14275AC50;
                chardataBinAddress = (IntPtr)0x142913470;
                dueldataBinAddress = (IntPtr)0x142919DA0;
                deckdataBinAddress = (IntPtr)0x1428FC080;
                cardPropsBinAddress = (IntPtr)0x142847E50;
                packDefDataBinAddress = (IntPtr)0x1429241C0;
                cardDataListStartAddress = (IntPtr)0x14275AB10;
                cardGenreBinAddress = (IntPtr)cardDataListStartAddress + 128;// Not really sure if this is right...
                duelLoadingScreenDelayAddress = (IntPtr)0x140A73858;
                coreDuelDataAddress = (IntPtr)0x14278EE68;
                duelPlayerInfoAddress = (IntPtr)0x143497C40;
                playerTurnAddress = (IntPtr)(0x14349B3B4 + 4);
                turnCountAddress = (IntPtr)0x14349B3C0;
                modeMatchDuelAddress = (IntPtr)0x140C8D35C;
                modeTagDuelAddress = (IntPtr)0x140C8D35D;
                modeTestOptionAddress = (IntPtr)0x140C8D360;
                modeNetworkedGameAddress = (IntPtr)0x140C8D1E6;
                modeNetworkedGameIsCreatorAddress = (IntPtr)0x140C8D1E7;
                modeNetworkedGameRankedAddress = (IntPtr)0x140C8D1E8;
                modeInstantStartDuelAddress = (IntPtr)0x140C8D1E9;
                modePlayerControllerAddress = (IntPtr)0x140C8D218;
                modeAIModeAddress = (IntPtr)0x140C8D39C;
                modeStartingLifePointsAddress = (IntPtr)0x140C8D370;
                modeStartingPlayerAddress = (IntPtr)0x140C8D384;
                modeTurnTimeLimitEnabledAddress = (IntPtr)0x140C8D364;
                modeTurnTimeLimitAddress = (IntPtr)0x140C8D368;
                modeTutorialDuelAddress = (IntPtr)0x140C8D1EA;
                modeTutorialDuelIndexAddress = (IntPtr)0x140C8D1EC;
                modeCampaignDuelAddress = (IntPtr)0x140C8D1D8;
                modeCampaignDuelIndexAddress = (IntPtr)0x140C8D1E0;
                modeCampaignDuelDeckIndexAddress = (IntPtr)0x140C8D1F4;
                modeBattlePackDuelAddress = (IntPtr)0x140C8D1D0;
                modeBattlePackIndexAddress = (IntPtr)0x140C8D1D4;
                modeChallengeDuelAddress = (IntPtr)0x140C8D1E5;
                modeMasterRules = (IntPtr)0x140C8D1C9;
                handSizeLimitAddress = (IntPtr)0x14005D1A2;
                popcornCoreDataAddress = (IntPtr)0x1429275D8;
                duelArenaAddress = (IntPtr)0x140C8D1F0;
                playerAvatarIdAddress = (IntPtr)0x140C8D1F8;
                playerDeckIdAddress = (IntPtr)0x140C8D208;
                saveDataAddress1 = (IntPtr)0x142924010;
                saveDataAddress2 = (IntPtr)0x142924148;
                defaultStructureDeckCardsAddress = (IntPtr)0x140A6C818;
                cardShopAddress = (IntPtr)0x1433280D0;
                queryPerformanceCounterAddress = (IntPtr)0x1409F9028;
                getTickCount64Address = (IntPtr)0x1409F9078;
                getTickCountAddress = (IntPtr)0;
                timeGetTimeAddress = (IntPtr)0;
                windowFocusPauseAddress = (IntPtr)0x14083C9F8;

                saveDataMemOffset = 80;// There isn't a seperate 80/88 on LE? Only 80?
                saveDataAddressOffset = 10208;// See sub_140871490
                popcornCoreOffset = 496;// See sub_1408121B0
                deckEditReturnScreenStateOffset = 60;// See sub_140822750
                deckEditBaseOffset = 584;// Somewhat guessed...
                deckEditTrunkOffset = 9984 + 720;// See sub_1408C0840
                deckEditUserDecksOffset = 6208 + 392 + 232;// See sub_1408C2620 / sub_1408967C0
                deckEditFilteredCardCountOffset = 516;
                deckEditCardCountOffset = 520;
                deckEditCardsOffset = 528;
                cardShopRevealedCardIdsOffset = 4448;// See sub_14085A2F0
                cardShopIsOpeningPackOffset = 4432;// See sub_14085C7C0
                dirtySaveDataOffset1 = 88;// See sub_140806950
                dirtySaveDataOffset2 = 92;// See sub_140806950
                dirtySaveDataOffset3 = 112;// See sub_140806950
                screenStateOffset1 = 856;// See sub_140808640
                screenStateOffset2 = 852;// See sub_140808640
                screenStateOffset3 = 48;// See sub_140808640
                hoveredCardOffset1 = 260;// See sub_1407C9640
                hoveredCardOffset2 = 8352;// See sub_14076FB50
                hoveredCardOffset3 = 5032;// See sub_14076FB50
                hoveredCardOffset4 = 48;// See sub_1407C9640
            }
        }
    }
}
