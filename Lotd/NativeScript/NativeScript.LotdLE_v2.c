#define popcornCoreDataAddress 0x1429275D8
#define popcornCoreOffset 496
#define screenStateFunctionAddress 0x1408087A0
#define coreDuelDataAddress 0x14278EE68
#define loadDuelFunctionAddress 0x140773220
#define setCardCollectionFunctionAddress 0x14088C490
#define deckEditDeckListSelectFunctionAddress 0x140868860
#define deckEditDeckListUpdateFunctionAddress 0x140896950
#define deckEditSelectedDeckUpdateFunctionAddress 0x140839B30
#define deckEditBaseOffset 584
#define deckEditUserDecksOffset 6208 + 392
#define deckEditUserDecksListOffset 232
#define deckEditUpdateUIFunctionAddress 0x14083A6C0
#define deckEditTrunkPanelOffset 704
#define deckEditRightPanelOffset 656
#define deckEditTrunkOffset 9984 + 720
#define cardShopAddress 0x1433280D0
#define cardShopSetStateFunctionAddress 0x14085D790
#define cardShopCardListOffset 0/*TODO*/
#define cardShopRefreshDuelPointsFunctionAddress 0x1408C1070
#define cardShopRefreshDuelPointsOffset 656
#define resizeStdVectorFunctionAddress 0x140754C00/*TODO: Make sure this is the right address (is a vector resize, unsure if right type)*/
#define playAudioFunctionAddress 0x14086C280
#define stopAudioFunctionAddress 0x14087A3C0
#define audioSnippetsAddress 0x143329748
#define loadBattlePackYdcFunctionAddress 0x1407BD130
#define loadBattlePackYdc_original { 0x48, 0x8B, 0xC4, 0x57, 0x48, 0x81, 0xEC, 0x60, 0x01, 0x00, 0x00, 0x48, 0xC7, 0x44 }
#define duelInitDeckHandLPFunctionAddress 0x14005FD90
#define duelInitDeckHandLP_original { 0x40, 0x53, 0x48, 0x83, 0xEC, 0x20, 0x8B, 0x0D, 0x48, 0xB6, 0x43, 0x03, 0xB8, 0x05 }
#define speedDuelAddress 0x14349B3E4 /* In this version a value of '2' exists which sets starting hand to 4, and regular LP... rush duels? TODO: test this! */
#define duelPlayerDataAddress 0x143497C40
#define duelPlayerDataSize 3476
#define duelPlayerDataNumCardsInHandOffset 12 /*TODO: Make sure this is up to date */
#define actionHandlerFunctionAddress 0x14000F400
#define actionHandler_original { 0x48, 0x89, 0x5C, 0x24, 0x10, 0x48, 0x89, 0x6C, 0x24, 0x18, 0x48, 0x89, 0x74, 0x24 }
#define animationHandlerFunctionAddress 0x1407C1450
#define animationHandler_original { 0x40, 0x53, 0x55, 0x56, 0x57, 0x41, 0x56, 0x48, 0x83, 0xEC, 0x60, 0x48, 0x8B, 0x05 }
#define duelThreadCriticalSectionAddress 0x1427CFDC0 /* Mtx_lock / Mtx_unlock in LE */
#define currentActionAddress 0x14332FA40
#define actionQueueAddress 0x14332FA40
#define hasActiveActionAddress 0x14349B4D0
#define numQueuedActionsAddress 0x143330250
#define currentActionState1Address 0x143330254
#define currentActionState2Address 0x143330258
#define drawCardFunctionAddress 0x140126750