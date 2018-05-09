// C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\bin\amd64\cl.exe
// C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\bin\dumpbin.exe
// 
// ---------------------------------------------------------------------
// --- The first line of this file is reserved for the compiler path ---
// --- The second line of this file is reserved for the dumpbin path ---
// ---------------------------------------------------------------------
// 
// This is tested on the VS2015 cl.exe compiler
//
// You can create distributable version of cl.exe (the compiler) by copying the following files from
// the Visual Studio bin directory "C:\Program Files (x86)\Microsoft Visual Studio XX.0\VC\bin\amd64"
// - If this isn't enough then just copy the entire contents of the folder
// 
// 1033/clui.dll
// c1.dll
// c2.dll
// cl.exe
// mspdbXXX.dll
//

typedef int Bool;
typedef long long Int64;
typedef unsigned long long UInt64;
typedef short Int16;
typedef unsigned short UInt16;
typedef int Int32;
typedef unsigned int UInt32;
typedef unsigned char UInt8;
typedef UInt16 TChar;

#if _M_IX86
typedef Int32 SizeT;
#else
typedef Int64 SizeT;
#endif

#define WINAPI __stdcall

#define NULL 0

#define false 0
#define true 1

// Some API functions
typedef void*(WINAPI *GetCurrentProcessDecl)();
typedef Bool(WINAPI *WriteProcessMemoryDecl)(void*, void*, void*, SizeT, SizeT*);
typedef Bool(WINAPI *VirtualProtectDecl)(void*, SizeT, Int32, Int32*);
typedef void(WINAPI *EnterCriticalSectionDecl)(void*);
typedef void(WINAPI *LeaveCriticalSectionDecl)(void*);
typedef void(__cdecl *srandDecl)(UInt32);
typedef Bool(WINAPI *QueryPerformanceCounterDecl)(Int64*);
typedef UInt64(WINAPI *GetTickCount64Decl)();
typedef Int32(WINAPI *GetTickCountDecl)();
typedef UInt32(*timeGetTimeDecl)();

#define NUM_MAIN_DECK_CARDS 60
#define NUM_SIDE_DECK_CARDS 15
#define NUM_EXTRA_DECK_CARDS 15

#define NUM_CARDS 7581

#define NUM_AUDIO_SNIPPETS 50

#define NUM_ACTION_IDS 0x70
#define NUM_ANIMATION_IDS 90

// Custom limit used for custom battle pack decks
#define NUM_CUSTOM_BATTLE_PACK_DECKS 100

typedef struct RandSeed
{
	UInt32 RandSeed1;
	UInt32 RandSeed2;
	UInt32 RandSeed3;
	UInt32 RandSeed4;
} RandSeed;

typedef struct DeckEditFilterCards
{
	Int32 NumFilteredCards;
	Int32 NumTotalCards;
	Int16 CardIds[NUM_CARDS];
} DeckEditFilterCards;

typedef struct CardShopOpenPackInfo
{
	Int32 NumCards;
	Int16 CardIds[8];
} CardShopOpenPackInfo;

typedef struct YdcDeck
{
	TChar DeckName[33];
	Int16 NumMainDeckCards;
	Int16 NumSideDeckCards;
	Int16 NumExtraDeckCards;
	Int16 MainDeck[NUM_MAIN_DECK_CARDS];
	Int16 SideDeck[NUM_SIDE_DECK_CARDS];
	Int16 ExtraDeck[NUM_EXTRA_DECK_CARDS];
	UInt8 Unk1[36];
	Int32 OwnerId;
	Int32 Unk2;
	Int32 Unk3;
	Int32 Available;
} YdcDeck;

typedef struct TimeMultiplierInfo
{
	// The initial values on first hook
	Int64 InitialPerformanceCounter;
	UInt64 InitialTickCount64;
	Int32 InitialTickCount;	
	UInt32 InitialTimeGetTime;
	
	// The offsets which are set every time the multiplier is changed
	Int64 OffsetPerformanceCounter;
	UInt64 OffsetTickCount64;
	Int32 OffsetTickCount;
	UInt32 OffsetTimeGetTime;
	
	double Multiplier;
	Int32 Enabled;
} TimeMultiplierInfo;

typedef struct Globals
{
	// Base address of NativeScript.c
	Int64 BaseAddress;

	// For writing over code without crashing
	GetCurrentProcessDecl GetCurrentProcess;
	WriteProcessMemoryDecl WriteProcessMemory;
	VirtualProtectDecl VirtualProtect;
	
	// For thread safe Action related things
	EnterCriticalSectionDecl EnterCriticalSection;
	LeaveCriticalSectionDecl LeaveCriticalSection;
	
	// For seeding the randomizer when loading a duel
	srandDecl srand;
	
	// For speeding up the game
	QueryPerformanceCounterDecl QueryPerformanceCounter;
	GetTickCount64Decl GetTickCount64;
	GetTickCountDecl GetTickCount;
	timeGetTimeDecl timeGetTime;
	
	TimeMultiplierInfo TimeMultiplier;
	
	RandSeed Seed;
	Int32 UseScreenStateTransitions;
	
	UInt8 BlockedActionIds[NUM_ACTION_IDS];// The ids are the index
	Int64 ActionHandlerHookAddress;// The address of the action handler hook in NativeScript.c	
	UInt8 BlockedAnimationIds[NUM_ANIMATION_IDS];// The ids are the index
	Int64 AnimationHandlerHookAddress;// The address of the animation handler hook in NativeScript.c
	Int32 CurrentAnimationId;// The currently updating animation id
	
	Int32 DuelSeed;
	
	Int64 DuelPostInitMemHookAddress;// For speed duel (otherwise it will be cleared)
	Int32 IsNextDuelSpeedDuel;
	Int32 NextDuelHandCount;// Number of cards to draw for the next duel
	
	Int64 LoadBattlePackYdcHookAddress;// For custom ydc battle packs
	Int32 IsCustomYdcBattlePacksEnabled;
	YdcDeck BattlePackDecks[NUM_CUSTOM_BATTLE_PACK_DECKS];
} Globals;

Globals* GetGlobals()
{
	// Magic number which will be replaced after compilation
	return (Globals*)0xAAAAAAAAAAAAAAAAUL;
}

double MillisecondsToSeconds(Int64 milliseconds);

UInt32 Rand()
{
	// "lfsr113.c" - Pierre L'Ecuyer

	Globals* globals = GetGlobals();
	UInt32 temp = 0;
	
	// Zero values aren't likely to produce nice results (will always produce 0 if they are all 0)
	// - "Initial seeds MUST be larger than 1, 3, 15, and 127 respectively"	
	if (globals->Seed.RandSeed1 <= 2) { globals->Seed.RandSeed1 = 12345; }
	if (globals->Seed.RandSeed2 <= 4) { globals->Seed.RandSeed2 = 12345; }
	if (globals->Seed.RandSeed3 <= 16) { globals->Seed.RandSeed3 = 12345; }
	if (globals->Seed.RandSeed4 <= 128) { globals->Seed.RandSeed4 = 12345; }
	
	temp = ((globals->Seed.RandSeed1 << 6) ^ globals->Seed.RandSeed1) >> 13;
	globals->Seed.RandSeed1 = ((globals->Seed.RandSeed1 & 0xFFFFFFFEU) << 18) ^ temp;
	temp = ((globals->Seed.RandSeed2 << 2) ^ globals->Seed.RandSeed2) >> 27;
	globals->Seed.RandSeed2 = ((globals->Seed.RandSeed2 & 0xFFFFFFF8U) << 2) ^ temp;
	temp = ((globals->Seed.RandSeed3 << 13) ^ globals->Seed.RandSeed3) >> 21;
	globals->Seed.RandSeed3 = ((globals->Seed.RandSeed3 & 0xFFFFFFF0U) << 7) ^ temp;
	temp = ((globals->Seed.RandSeed4 << 3) ^ globals->Seed.RandSeed4) >> 12;
	globals->Seed.RandSeed4 = ((globals->Seed.RandSeed4 & 0xFFFFFF80U) << 13) ^ temp;
	return (globals->Seed.RandSeed1 ^ globals->Seed.RandSeed2 ^ globals->Seed.RandSeed3 ^ globals->Seed.RandSeed4);
}

void WriteProcessMemoryEx(void* lpBaseAddress, void* buffer, SizeT nSize, SizeT* lpNumberOfBytesWritten)
{
	Globals* g = GetGlobals();
	void* hProcess = g->GetCurrentProcess();
	
	if (!g->WriteProcessMemory(hProcess, lpBaseAddress, buffer, nSize, lpNumberOfBytesWritten))
	{
		// We probably don't have access to this page. Change the page protection to gain access
		Int32 oldPageProtection;
		if(!g->VirtualProtect(lpBaseAddress, nSize, 0x40, &oldPageProtection))
		{
			// We failed to obtain access
			return;
		}
		
		g->WriteProcessMemory(hProcess, lpBaseAddress, buffer, nSize, lpNumberOfBytesWritten);
		
		// Restore the old page protection
		g->VirtualProtect(lpBaseAddress, nSize, oldPageProtection, &oldPageProtection);
	}
}

void UnsafeHook(Int64 address, Int64 hook)
{
	UInt8 hookBytes[14];
	hookBytes[0] = 0xFF;
	hookBytes[1] = 0x25;
	hookBytes[2] = 0x00;
	hookBytes[3] = 0x00;
	hookBytes[4] = 0x00;
	hookBytes[5] = 0x00;
	*(Int64*)((Int64)((UInt8*)hookBytes) + 6) = hook;
	
	SizeT written;
	WriteProcessMemoryEx((void*)address, (void*)hookBytes, 14, &written);
}

void UnsafeUnHook(Int64 address, UInt8* originalBytes, Int32 length)
{
	SizeT written;
	WriteProcessMemoryEx((void*)address, (void*)originalBytes, length, &written);
}

Int64 GetPopcornCoreAddress()
{
	// See main_data_address.c
	Int64 popcornCore = *(Int64*)0x140EA2148;
	popcornCore = *(Int64*)(popcornCore + 472);
	return popcornCore;
}

typedef struct ScreenStateInfo
{
	Int32 State;
	Int32 TransitionType;
	double TransitionTime;
} ScreenStateInfo;

typedef char(__fastcall *SetScreenStateDecl)(Int64, Int32, double, Int32, char);
void SetScreenState(ScreenStateInfo* screenStateInfo)
{
	// See YGOPopcornCore_loadduel.c / loadduel_addresses.c
	Int64 screenStateFunctionAddress = 0x14062D140;
	Int64 popcornCoreAddress = GetPopcornCoreAddress();
	
	// The screen state id
	Int32 screenStateId = screenStateInfo->State;
	
	// Transition type determines how the game should fade the screen and what colour (black/white)
	Int32 transitionType = screenStateInfo->TransitionType;
	
	// Duration that the transition takes to complete (input is ignored during this time)
	double transitionTime = screenStateInfo->TransitionTime;
	
	((SetScreenStateDecl)screenStateFunctionAddress)(
		popcornCoreAddress, screenStateId, transitionTime, transitionType, 1);
}

void SetScreenStateId(Int32 screenStateId)
{
	ScreenStateInfo screenStateInfo;
	screenStateInfo.State = screenStateId;
	screenStateInfo.TransitionType = 273;
	if (GetGlobals()->UseScreenStateTransitions)
	{
		// default transition time
		screenStateInfo.TransitionTime = MillisecondsToSeconds(150);
	}
	else
	{
		screenStateInfo.TransitionTime = 0;
	}
	SetScreenState(&screenStateInfo);
}

typedef Int32(__fastcall *LoadDuelDecl)(Int64);
void LoadDuel(void* unused)
{
	Globals* g = GetGlobals();
	
	// We need to seed srand as the game uses rand() to initialize a seed used to randomize the cards in the
	// card list but the game seems to reset srand somewhere (although there is no obvious call which resets it)
	// - See YGOWorkerThread_loadduel.c - sub_1406BF4E0
	g->srand(g->DuelSeed);//g->srand(Rand());

	// See YGOWorkerThread_loadduel.c / loadduel_addresses.c
	Int64 loadDuelFunctionAddress = 0x140894220;
	Int64 coreDuelDataAddress = 0x1410CF430;
	
	((LoadDuelDecl)loadDuelFunctionAddress)(*(Int64*)coreDuelDataAddress);	
}

void LoadAndStartDuel(void* unused)
{
	LoadDuel(unused);
	
	// Set screen state to "loading screen finished"
	SetScreenStateId(11);
}

// Used for deck editor trunk cards / card shop revealed cards / likely other things
typedef Int32(__fastcall *SetCardCollectionDecl)(Int64, Int32, Int16*, Int32, Int64, Int64);
void SetCardCollection(Int64 cardCollectionAddress, Int32 numCards, Int16* cardIds, Int32 numTotalCards)
{
	// See deckEditApplyCardFilter.c / cardShopOpenPack.c
	Int64 setCardCollectionFunctionAddress = 0x1406576B0;
	
	// last two parameters are related to determining how to calculate the number of available cards
	// relative to the cards in use
	((SetCardCollectionDecl)setCardCollectionFunctionAddress)(
		cardCollectionAddress, numCards, cardIds, numTotalCards, NULL, NULL);
}

typedef void(__fastcall *DeckEditorDeckListSelectDecl)(Int64, Int32);
typedef void(__fastcall *DeckEditorDeckListUpdateDecl)(Int64);
typedef void(__fastcall *DeckEditorSelectedDeckUpdateDecl)(Int64);
void DeckEditSelectDeck(Int64 deckIndex)
{
	// See deckEditHoverUserDeck.c / deckEditOpenTrunk.c
	Int64 deckEditDeckListSelectFunctionAddress = 0x1406BBD10;
	Int64 deckEditDeckListUpdateFunctionAddress = 0x14065EB10;
	Int64 deckEditSelectedDeckUpdateFunctionAddress = 0x14064DA30;
	Int64 popcornCoreAddress = GetPopcornCoreAddress();
	Int64 deckEditBaseAddress = *(Int64*)(popcornCoreAddress + 608);	
	Int64 deckEditUserDecksAddress = deckEditBaseAddress + 5424 + 376;
	Int64 deckEditUserDecksListAddress = deckEditUserDecksAddress + 232;
	
	// Set the given deck index to "selected" in the list then update the UI to reflect that change
	((DeckEditorDeckListSelectDecl)deckEditDeckListSelectFunctionAddress)(deckEditUserDecksListAddress, (Int32)deckIndex);
	((DeckEditorDeckListUpdateDecl)deckEditDeckListUpdateFunctionAddress)(deckEditUserDecksAddress);
	
	// Call the function to update the deck info for the selected deck
	// (this will actually fill out the info for the deck using the info from the selected deck index)
	((DeckEditorSelectedDeckUpdateDecl)deckEditSelectedDeckUpdateFunctionAddress)(deckEditBaseAddress);
}

typedef void(__fastcall *DeckEditorUpdateUIDecl)(Int64);
void OpenDeckEditTrunkPanel()
{
	// See deckEditOpenTrunk.c / deckEditApplyCardFilter.c
	Int64 popcornCoreAddress = GetPopcornCoreAddress();
	Int64 deckEditBaseAddress = *(Int64*)(popcornCoreAddress + 608);
	Int64 deckEditUpdateUIFunctionAddress = 0x14064E6D0;
	
	// Open the trunk panel
	*(Int32*)(deckEditBaseAddress + 680) = 2;// set the trunk panel to open?
	*(Int32*)(deckEditBaseAddress + 632) = 2;// focus the right side panel?
	((DeckEditorUpdateUIDecl)deckEditUpdateUIFunctionAddress)(deckEditBaseAddress);
}

void SetDeckEditTrunkCards(DeckEditFilterCards* filterCards)
{
	// See deckEditApplyCardFilter.c
	Int64 popcornCoreAddress = GetPopcornCoreAddress();
	Int64 deckEditBaseAddress = (*(Int64*)(popcornCoreAddress + 608));
	Int64 deckEditTrunkAddress = deckEditBaseAddress + 9040 + 728;
	
	SetCardCollection(deckEditTrunkAddress, filterCards->NumFilteredCards,
		filterCards->CardIds, filterCards->NumTotalCards);
}

typedef Int32(__fastcall *CardShopSetStateDecl)(Int64, Int32, float);
void CardShopOpenPack(CardShopOpenPackInfo* packInfo)
{
	// NOTE: We aren't adding any cards to the owned cards list here, that is handled elsewhere
	//       therefore this is only the visuals of opening a pack

	// See cardShopOpenPack.c
	Int64 cardShopAddress = *(Int64*)0x140EA21A0;
	Int64 cardShopSetStateFunctionAddress = 0x140653910;
	
	// Sets the card list which are browsable once the cards are revealed
	// (you can technically have a different set of cards to the ones revealed)
	SetCardCollection(cardShopAddress + 2144, packInfo->NumCards, packInfo->CardIds, -1, NULL, NULL);
	
	// Set the card shop state to "unwrap" (2) and then to revealing the cards (3)
	// - "unwrap" is required otherwise the cards are displayed on top of the card shop
	((CardShopSetStateDecl)cardShopSetStateFunctionAddress)(cardShopAddress, 2, 0);
	((CardShopSetStateDecl)cardShopSetStateFunctionAddress)(cardShopAddress, 3, 0);
}

typedef Int32(__fastcall *CardShopRefreshDuelPointsDecl)(Int64);
void CardShopRefreshDuelPoints(void* unused)
{	
	Int64 cardShopAddress = *(Int64*)0x140EA21A0;
	Int64 cardShopRefreshDuelPointsFunctionAddress = 0x140685C20;
	
	// See cardShopOpenPack.c sub_1406522E0 / sub_140685C20
	((CardShopRefreshDuelPointsDecl)cardShopRefreshDuelPointsFunctionAddress)(cardShopAddress + 632);
}

typedef Int64(__fastcall *ResizeStdVectorDecl)(Int64, Int64);
Int64 InitCardShopPackOpener(Int64 address)
{
	// This should do a resize on the std::vector for the card shop cards address
	Int64 resizeStdVectorFunctionAddress = 0x1405DE010;	
	return ((ResizeStdVectorDecl)resizeStdVectorFunctionAddress)(address, 8);
}

typedef char(__fastcall *PlayAudioDecl)(Int32);
void PlayAudio(Int64 audioIndex)
{
	// See audio.c
	Int64 playAudioFunctionAddress = 0x1405EEF70;
	
	if (audioIndex < NUM_AUDIO_SNIPPETS)
	{
		((PlayAudioDecl)playAudioFunctionAddress)((Int32)audioIndex);
	}
}

typedef void(__fastcall *StopAudioDecl)(Int64, char);
void StopAudio(Int64 audioIndex)
{
	// See audio.c
	Int64 stopAudioFunctionAddress = 0x1408F94D0;
	Int64* audioSnippetsAddress = (Int64*)0x140E8B140;
	
	if (audioIndex < NUM_AUDIO_SNIPPETS)
	{
		Int64 audioSnippet = audioSnippetsAddress[audioIndex];
		if (audioSnippet)
		{
			// Note that will crash if you provide an index beyond the array length
			((StopAudioDecl)stopAudioFunctionAddress)(audioSnippet, 0);
		}
	}
}

void StopAllAudio(void* unused)
{
	for (int i = 0; i < NUM_AUDIO_SNIPPETS; i++)
	{
		StopAudio(i);
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////
// Hook for custom battle pack ydc loading (the AI decks)
////////////////////////////////////////////////////////////////////////////////////////////////

Int64 GetLoadBattlePackYdcFunctionAddress()
{
	// YGOWorkerThread_loadduel.c / memory.c (sub_1406BEC60 sub_14088E9E0)
	return 0x14088E9E0;
}

void HookLoadBattlePackYdc()
{
	UnsafeHook(GetLoadBattlePackYdcFunctionAddress(), GetGlobals()->LoadBattlePackYdcHookAddress);
}

void UnhookLoadBattlePackYdc()
{
	UInt8 originalBytes[14] = { 0x48, 0x8b, 0xc4, 0x57, 0x48, 0x81, 0xec, 0x60, 0x01, 0x00, 0x00, 0x48, 0xc7, 0x44 };
	UnsafeUnHook(GetLoadBattlePackYdcFunctionAddress(), originalBytes, 14);
}

typedef Int32(__fastcall *LoadBattlePackYdcDecl)(TChar*, YdcDeck*, Int64, Int64);
Int32 LoadBattlePackYdc_hook(TChar* a1, YdcDeck* a2, Int64 a3, Int64 a4)
{
	Globals* g = GetGlobals();
	
	Int32 numAvailableDecks = 0;
	for (Int32 i = 0; i < NUM_CUSTOM_BATTLE_PACK_DECKS; i++)
	{
		if (!g->BattlePackDecks[i].Available)
		{
			break;
		}
		numAvailableDecks++;
	}
	
	if (!g->IsCustomYdcBattlePacksEnabled || numAvailableDecks == 0)
	{
		UnhookLoadBattlePackYdc();
		Int32 result = ((LoadBattlePackYdcDecl)GetLoadBattlePackYdcFunctionAddress())(a1, a2, a3, a4);
		HookLoadBattlePackYdc();
		return result;
	}
	
	*a2 = g->BattlePackDecks[Rand() % numAvailableDecks];
	return 1;
}

////////////////////////////////////////////////////////////////////////////////////////////////
// Hook for speed duel mode (otherwise it is reset by a memset)
// - The target function should be within the context of a lock on the "YGO:funcThreadDuel" critical section
//   as we hook with that critical section it should be thread safe - see YGOPopcornCore_beginduel_setDuelInfo.c
////////////////////////////////////////////////////////////////////////////////////////////////

Int64 GetDuelInitDeckHandLPFunctionAddress()
{
	// See YGOPopcornCore_beginduel_setDuelInfo.c
	return 0x140080A80;
}

void HookDuelInitDeckHandLP()
{
	UnsafeHook(GetDuelInitDeckHandLPFunctionAddress(), GetGlobals()->DuelPostInitMemHookAddress);
}

void UnhookDuelInitDeckHandLP()
{
	UInt8 originalBytes[14] = { 0x40, 0x53, 0x48, 0x83, 0xec, 0x20, 0x83, 0x3d, 0x7f, 0x00, 0x10, 0x01, 0x00, 0xb8 };
	UnsafeUnHook(GetDuelInitDeckHandLPFunctionAddress(), originalBytes, 14);
}

typedef Int64(__fastcall *DuelInitDeckHandLPDecl)();
Int64 DuelInitDeckHandLP_hook()
{	
	Int64 speedDuelAddress = 0x141180B0C;
	Int64 duelPlayerDataAddress = 0x14117D580;
	Int64 duelPlayerDataSize = 3472;
	Int64 duelPlayerDataNumCardsInHandOffset = 12;
	Int64 duelInitDeckHandLPFunctionAddress = GetDuelInitDeckHandLPFunctionAddress();
	
	Globals* g = GetGlobals();
	
	// Set speed duel mode if enabled
	if (g->IsNextDuelSpeedDuel)
	{
		g->IsNextDuelSpeedDuel = false;
		*((UInt8*)speedDuelAddress) = 1;
	}
	
	UnhookDuelInitDeckHandLP();
	Int64 result = ((DuelInitDeckHandLPDecl)duelInitDeckHandLPFunctionAddress)();
	HookDuelInitDeckHandLP();
	
	// The hand count was just set, replace it with a custom hand count if enabled
	if (g->NextDuelHandCount >= 0)
	{
		*(Int32*)(duelPlayerDataAddress + (duelPlayerDataSize * 0) + duelPlayerDataNumCardsInHandOffset) = g->NextDuelHandCount;
		*(Int32*)(duelPlayerDataAddress + (duelPlayerDataSize * 1) + duelPlayerDataNumCardsInHandOffset) = g->NextDuelHandCount;
		g->NextDuelHandCount = -1;
	}
	
	// If we wanted cards to appear in a certain order this would be a good time do that
	// as the deck was just randomized in the hooked call
	
	return result;
}

////////////////////////////////////////////////////////////////////////////////////////////////
// These "Action" functions are to saftely use actions by aquiring the critical section of the
// "YGO:funcThreadDuel" thread before changing any data.
////////////////////////////////////////////////////////////////////////////////////////////////

typedef struct ActionState
{
	Int32 State1;
	Int32 State2;
} ActionState;

typedef struct ActionElement
{
	UInt16 Action;
	UInt8 ActionData[6];
} ActionElement;

typedef struct ActionInfo
{
	Int32 SetState;// bool
	
	ActionState State;// Defines the action state
	ActionElement Action;// Defines the action data
	
	Int32 Type;
	
	// Defines count for ACTION_TYPE_FORCED
	// Defines inject index for ACTION_TYPE_INJECT
	Int32 CustomData;
} ActionInfo;

#define ACTION_TYPE_NONE 0
#define ACTION_TYPE_QUEUE 1
#define ACTION_TYPE_OVERWRITE 2
#define ACTION_TYPE_INJECT 3
#define ACTION_TYPE_FORCED 4
#define ACTION_TYPE_CLEAR_CURRENT 5
#define ACTION_TYPE_CLEAR_QUEUE 6
#define ACTION_TYPE_CLEAR_ALL 7
#define ACTION_TYPE_INIT_HOOKS 8

Int64 GetActionHandlerFunctionAddress()
{
	return 0x1401012D0;
}

void HookActionHandler()
{
	UnsafeHook(GetActionHandlerFunctionAddress(), GetGlobals()->ActionHandlerHookAddress);
}

void UnhookActionHandler()
{
	UInt8 originalBytes[14] = { 0x40, 0x53, 0x48, 0x83, 0xec, 0x30, 0x0f, 0xb7, 0x0d, 0xe3, 0x6c, 0xfd, 0x00, 0x41 };
	UnsafeUnHook(GetActionHandlerFunctionAddress(), originalBytes, 14);
}

Int64 GetAnimationHandlerFunctionAddress()
{
	return 0x140881DB0;
}

void HookAnimationHandler()
{
	UnsafeHook(GetAnimationHandlerFunctionAddress(), GetGlobals()->AnimationHandlerHookAddress);
}

void UnhookAnimationHandler()
{
	UInt8 originalBytes[14] = { 0x48, 0x89, 0x5c, 0x24, 0x08, 0x48, 0x89, 0x6c, 0x24, 0x10, 0x48, 0x89, 0x74, 0x24 };
	UnsafeUnHook(GetAnimationHandlerFunctionAddress(), originalBytes, 14);
}

void* GetDuelThreadCriticalSection()
{
	// See YGOfuncThreadDuel.c (they lock the critical section once per cycle)
	return (void*)0x1410D4C70;
}

// I think this is the correct function signature?
// Not 100% sure what the parameter values are, they are mostly unused by action handlers.
typedef void(__fastcall *ProcessActionDecl)(Int64, Int64, float);
void ProcessAction(Int64 a1, Int64 a2, float a3)
{
	// See  YGOfuncThreadDuel.c
	Int64 processActionFunctionAddress = 0x1401012D0;
	
	((ProcessActionDecl)processActionFunctionAddress)(a1, a2, a3);
}

void DoAction(ActionInfo* actionInfo)
{
	Globals* g = GetGlobals();

	// See YGOfuncThreadDuel.c
	ActionElement* currentActionPtr = (ActionElement*)0x1410D7FC0;
	ActionElement* actionQueuePtr = (ActionElement*)0x1410D7FC8;
	Int32* hasActiveActionPtr = (Int32*)0x141180BF8;
	Int32* numQueuedActionsPtr = (Int32*)0x1410D87C8;
	Int32* currentActionState1Ptr = (Int32*)0x1410D87CC;
	Int32* currentActionState2Ptr = (Int32*)0x1410D87D0;
	Int64 drawCardFunctionAddress = 0x14049BA30;
	
	void* duelThreadCriticalSection = GetDuelThreadCriticalSection();

	if (g->EnterCriticalSection == NULL || g->LeaveCriticalSection == NULL ||
		duelThreadCriticalSection == NULL)
	{
		return;
	}
	
	g->EnterCriticalSection(duelThreadCriticalSection);
	
	// ACTION_TYPE_FORCED will handle state itself
	if (actionInfo->SetState && actionInfo->Type != ACTION_TYPE_FORCED)
	{
		*currentActionState1Ptr = actionInfo->State.State1;
		*currentActionState2Ptr = actionInfo->State.State2;
	}
	
	Bool hasActiveAction = *hasActiveActionPtr;
	Int32 numQueuedActions = *numQueuedActionsPtr;
	
	// NOTE: ACTION_TYPE_OVERWRITE / ACTION_TYPE_INJECT (at index 0) are both unsafe as
	// they will both potentially overwrite partially complete action/state data

	if (actionInfo->Type == ACTION_TYPE_QUEUE)
	{
		actionQueuePtr[numQueuedActions] = actionInfo->Action;
		*numQueuedActionsPtr = numQueuedActions + 1;
	}
	else if(actionInfo->Type == ACTION_TYPE_OVERWRITE)
	{
		*currentActionPtr = actionInfo->Action;
	}
	else if(actionInfo->Type == ACTION_TYPE_INJECT)
	{
		Int32 injectIndex = actionInfo->CustomData;
		
		if (numQueuedActions == 0 && !hasActiveAction)
		{
			// We don't want to inject at index 0 if the queue is empty and there is no active action.
			// Just push our action onto the queue as the next index and let the queue handler pick it up.
			injectIndex = 1;
		}
		
		// Make sure inject index doesn't go beyond the number of queued actions			
		if (injectIndex > numQueuedActions)
		{
			// This will inject the action at the end of the queue
			// +1 as numQueuedActions doesn't include the active action in the count
			injectIndex = numQueuedActions + 1;
		}
		
		// Move existing actions upward once place which are >= the target index
		for (Int32 i = numQueuedActions; i >= injectIndex; i--)
		{
			currentActionPtr[i + 1] = currentActionPtr[i];
		}
		
		// Set our action at the desired index and update the queue count
		currentActionPtr[injectIndex] = actionInfo->Action;
		*numQueuedActionsPtr = numQueuedActions + 1;
	}
	else if(actionInfo->Type == ACTION_TYPE_FORCED && actionInfo->CustomData > 0)
	{
		ActionElement currentElement = *currentActionPtr;
		Int32 currentActionState1 = *currentActionState1Ptr;			
		Int32 currentActionState2 = *currentActionState2Ptr;
		
		// Swap the current action with the one to be injected
		*currentActionPtr = actionInfo->Action;
		
		// Here we want to force drawing of a card x number of times
		for (Int32 i = 0; i < actionInfo->CustomData; i++)
		{
			if (actionInfo->SetState)
			{
				*currentActionState1Ptr = actionInfo->State.State1;
				*currentActionState2Ptr = actionInfo->State.State2;
			}
			
			// Process the current action
			ProcessAction(0, 0, 0);
		}
		
		// Restore the real data
		*currentActionPtr = currentElement;
		*currentActionState1Ptr = currentActionState1;
		*currentActionState2Ptr = currentActionState2;
	}
	else if(actionInfo->Type == ACTION_TYPE_CLEAR_CURRENT)
	{
		*hasActiveActionPtr = 0;
	}
	else if(actionInfo->Type == ACTION_TYPE_CLEAR_QUEUE)
	{
		*numQueuedActionsPtr = 0;
	}
	else if(actionInfo->Type == ACTION_TYPE_CLEAR_ALL)
	{
		*hasActiveActionPtr = 0;
		*numQueuedActionsPtr = 0;
	}
	else if(actionInfo->Type == ACTION_TYPE_INIT_HOOKS)
	{
		HookActionHandler();
		HookAnimationHandler();
		HookDuelInitDeckHandLP();
		HookLoadBattlePackYdc();
	}
	
	g->LeaveCriticalSection(duelThreadCriticalSection);
}

typedef void(__fastcall *ActionHandlerDecl)(Int64, Int64);
void __fastcall ActionHandler_hook(Int64 a1, Int64 a2)
{
	Int64 actionHandlerFunctionAddress = GetActionHandlerFunctionAddress();
	ActionElement* currentActionPtr = (ActionElement*)0x1410D7FC0;
	Int32* hasActiveActionPtr = (Int32*)0x141180BF8;
	
	Globals* g = GetGlobals();
	
	UInt16 actionId = currentActionPtr->Action & 0xFFF;
	if (actionId < NUM_ACTION_IDS && g->BlockedActionIds[actionId] != 0)
	{
		*hasActiveActionPtr = 0;
	}
	else
	{
		UnhookActionHandler();
		((ActionHandlerDecl)actionHandlerFunctionAddress)(a1, a2);
		HookActionHandler();
	}
}

typedef Int64(__fastcall *AnimationHandlerDecl)(Int32, Int32, Int32, Int32, float);
Int64 __fastcall AnimationHandler_hook(Int32 animationId, Int32 a2, Int32 a3, Int32 a4, float a5)
{
	Int64 animationHandlerFunctionAddress = GetAnimationHandlerFunctionAddress();
	
	Globals* g = GetGlobals();
	g->CurrentAnimationId = animationId;
	
	if (animationId < NUM_ANIMATION_IDS && g->BlockedAnimationIds[animationId] != 0)
	{
		return 0;
	}
	else
	{
		UnhookAnimationHandler();
		Int64 result = ((AnimationHandlerDecl)animationHandlerFunctionAddress)(animationId, a2, a3, a4, a5);
		HookAnimationHandler();
		
		return result;
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////
// Functions for speeding up the game
////////////////////////////////////////////////////////////////////////////////////////////////

Bool WINAPI QueryPerformanceCounter_hook(Int64* lpPerformanceCount)
{
	Globals* g = GetGlobals();
	TimeMultiplierInfo timeMultiplier = g->TimeMultiplier;
	
	Int64 currentTime;
	Bool result = g->QueryPerformanceCounter(&currentTime);
	
	currentTime = ((currentTime - timeMultiplier.InitialPerformanceCounter) * timeMultiplier.Multiplier) +
		timeMultiplier.OffsetPerformanceCounter;
	
	*lpPerformanceCount = currentTime;
	
	return result;
}

UInt64 WINAPI GetTickCount64_hook()
{
	Globals* g = GetGlobals();
	TimeMultiplierInfo timeMultiplier = g->TimeMultiplier;
	
	UInt64 currentTime = g->GetTickCount64();
	
	currentTime = ((currentTime - timeMultiplier.InitialTickCount64) * timeMultiplier.Multiplier) +
		timeMultiplier.OffsetTickCount64;
	
	return currentTime;
}

Int32 WINAPI GetTickCount_hook()
{
	Globals* g = GetGlobals();
	TimeMultiplierInfo timeMultiplier = g->TimeMultiplier;
	
	Int32 currentTime = g->GetTickCount();
	
	currentTime = ((currentTime - timeMultiplier.InitialTickCount) * timeMultiplier.Multiplier) +
		timeMultiplier.OffsetTickCount;
	
	return currentTime;
}

UInt32 timeGetTime_hook()
{
	Globals* g = GetGlobals();
	TimeMultiplierInfo timeMultiplier = g->TimeMultiplier;
	
	UInt32 currentTime = g->timeGetTime();
	
	currentTime = ((currentTime - timeMultiplier.InitialTimeGetTime) * timeMultiplier.Multiplier) +
		timeMultiplier.OffsetTimeGetTime;
	
	return currentTime;
}

void SetTimeMultiplierInfo(TimeMultiplierInfo timeMultiplier)
{
	GetGlobals()->TimeMultiplier = timeMultiplier;
}

typedef struct StructSizeInfo
{
	Int32 Self;
	Int32 Globals;
	Int32 RandSeed;
	Int32 TimeMultiplierInfo;
	Int32 ScreenStateInfo;
	Int32 ActionState;
	Int32 ActionElement;
	Int32 ActionInfo;
	Int32 DeckEditFilterCards;
	Int32 CardShopOpenPackInfo;
} StructSizeInfo;

void GetStructSizeInfo(StructSizeInfo* sizeInfo)
{
	sizeInfo->Self = sizeof(StructSizeInfo);
	sizeInfo->Globals = sizeof(Globals);
	sizeInfo->RandSeed = sizeof(RandSeed);
	sizeInfo->TimeMultiplierInfo = sizeof(TimeMultiplierInfo);
	sizeInfo->ScreenStateInfo = sizeof(ScreenStateInfo);
	sizeInfo->ActionState = sizeof(ActionState);
	sizeInfo->ActionElement = sizeof(ActionElement);
	sizeInfo->ActionInfo = sizeof(ActionInfo);
	sizeInfo->DeckEditFilterCards = sizeof(DeckEditFilterCards);
	sizeInfo->CardShopOpenPackInfo = sizeof(CardShopOpenPackInfo);
}

Int32 EntryPoint(void* unused)
{
	return 0;
}
// The "void* unused" parameter exists so that they match the signature for CreateRemoteThread

////////////////////////////////////////////////////////////////////////////////////////////////
// Avoid turning on optimizations when compiling this
////////////////////////////////////////////////////////////////////////////////////////////////
// The compiler likes to uses instructions like movsd, movss, divsd for floating point values.
// Constants will be put into a data section which needs to be handled by the compiler / linker.
// As we have no data section we can't have floating point constants.
////////////////////////////////////////////////////////////////////////////////////////////////
double MillisecondsToSeconds(Int64 milliseconds)
{	
	Int32 div = 1000;
	return milliseconds / (double)div;
}