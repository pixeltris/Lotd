#define popcornCoreDataAddress 0x140EA2148
#define popcornCoreOffset 472
#define screenStateFunctionAddress 0x14062D140
#define coreDuelDataAddress 0x1410CF430
#define loadDuelFunctionAddress 0x140894220
#define setCardCollectionFunctionAddress 0x1406576B0
#define deckEditDeckListSelectFunctionAddress 0x1406BBD10
#define deckEditDeckListUpdateFunctionAddress 0x14065EB10
#define deckEditSelectedDeckUpdateFunctionAddress 0x14064DA30
#define deckEditBaseOffset 608
#define deckEditUserDecksOffset 5424 + 376
#define deckEditUserDecksListOffset 232
#define deckEditUpdateUIFunctionAddress 0x14064E6D0
#define deckEditTrunkPanelOffset 680
#define deckEditRightPanelOffset 632
#define deckEditTrunkOffset 9040 + 728
#define cardShopAddress *(Int64*)0x140EA21A0
#define cardShopSetStateFunctionAddress 0x140653910
#define cardShopCardListOffset 2144
#define cardShopRefreshDuelPointsFunctionAddress 0x140685C20
#define cardShopRefreshDuelPointsOffset 632
#define resizeStdVectorFunctionAddress 0x1405DE010
#define playAudioFunctionAddress 0x1405EEF70
#define stopAudioFunctionAddress 0x1408F94D0
#define audioSnippetsAddress 0x140E8B140
#define loadBattlePackYdcFunctionAddress 0x14088E9E0
#define loadBattlePackYdc_original { 0x48, 0x8b, 0xc4, 0x57, 0x48, 0x81, 0xec, 0x60, 0x01, 0x00, 0x00, 0x48, 0xc7, 0x44 }
#define duelInitDeckHandLPFunctionAddress 0x140080A80
#define duelInitDeckHandLP_original { 0x40, 0x53, 0x48, 0x83, 0xec, 0x20, 0x83, 0x3d, 0x7f, 0x00, 0x10, 0x01, 0x00, 0xb8 }
#define speedDuelAddress 0x141180B0C
#define duelPlayerDataAddress 0x14117D580
#define duelPlayerDataSize 3472
#define duelPlayerDataNumCardsInHandOffset 12
#define actionHandlerFunctionAddress 0x1401012D0
#define actionHandler_original { 0x40, 0x53, 0x48, 0x83, 0xec, 0x30, 0x0f, 0xb7, 0x0d, 0xe3, 0x6c, 0xfd, 0x00, 0x41 }
#define animationHandlerFunctionAddress 0x140881DB0
#define animationHandler_original { 0x48, 0x89, 0x5c, 0x24, 0x08, 0x48, 0x89, 0x6c, 0x24, 0x10, 0x48, 0x89, 0x74, 0x24 }
#define duelThreadCriticalSectionAddress 0x1410D4C70
#define currentActionAddress 0x1410D7FC0
#define actionQueueAddress 0x1410D7FC8
#define hasActiveActionAddress 0x141180BF8
#define numQueuedActionsAddress 0x1410D87C8
#define currentActionState1Address 0x1410D87CC
#define currentActionState2Address 0x1410D87D0
#define drawCardFunctionAddress 0x14049BA30