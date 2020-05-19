using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Lotd
{
    public partial class MemTools
    {
        // For randomizing seed
        private Random rand = new Random();

        private Addresses_Lotd addresses;

        // Cache addresses of custom battle pack data
        private Dictionary<string, IntPtr> customBattlePackDataAddresses = new Dictionary<string, IntPtr>();
        private int customBattlePackMaxDataLen = 8192;

        private bool processWatcherRunning = false;

        /// <summary>
        /// Determines if transitions should be used for screen state changes
        /// </summary>
        public bool UseScreenStateTransitions { get; set; }

        private bool customYdcBattlePacksEnabled;
        public bool CustomYdcBattlePacksEnabled
        {
            get { return customYdcBattlePacksEnabled; }
            set
            {
                if (IsNativeScriptLoaded)
                {
                    WriteValue<int>(nativeScriptGlobalsAddress + NativeScript.Globals.OffsetCustomYdcBattlePacksEnabled,
                        value ? 1 : 0);
                }
                customYdcBattlePacksEnabled = value;
            }
        }

        public bool IsFullyLoaded { get; private set; }
        public event EventHandler Loaded;
        public event EventHandler Unloaded;

        public GameVersion Version { get; private set; }

        public MemTools(GameVersion version)
        {
            Version = version;

            switch (version)
            {
                case GameVersion.Lotd:
                    addresses = new Addresses_Lotd();
                    break;
                case GameVersion.LinkEvolution1:
                    addresses = new Addresses_LotdLE_v1();
                    break;
                case GameVersion.LinkEvolution2:
                    addresses = new Addresses_LotdLE_v2();
                    break;
            }
            addresses.Mem = this;

            ValidateStructs();
            UseScreenStateTransitions = true;
        }

        private void ValidateStructs()
        {
            Debug.Assert(Marshal.SizeOf(typeof(YdcDeck)) == 304);
            Debug.Assert(Marshal.SizeOf(typeof(YdcDeck)) == GameSaveData.GetDecksSize(Version) / Constants.NumUserDecks);
            Debug.Assert(Marshal.SizeOf(typeof(CardProps)) == 48);
            Debug.Assert(Marshal.SizeOf(typeof(RawPackDefData)) == 56);// TODO: Update for LE?
            Debug.Assert(Marshal.SizeOf(typeof(DuelPlayerInfo)) == 3472);// LE = 3476. TODO: Update for LE
        }

        private void ValidateNativeScriptStructs()
        {
            if (!IsNativeScriptLoaded)
            {
                return;
            }

            NativeScript.StructSizeInfo sizeInfo = new NativeScript.StructSizeInfo();
            if (CallNativeScriptFunctionWithStruct("GetStructSizeInfo", ref sizeInfo) == CallNativeFunctionResult.Success)
            {
                Debug.Assert(sizeInfo.Self == Marshal.SizeOf(typeof(NativeScript.StructSizeInfo)));
                Debug.Assert(sizeInfo.Globals == Marshal.SizeOf(typeof(NativeScript.Globals)));
                Debug.Assert(sizeInfo.RandSeed == Marshal.SizeOf(typeof(NativeScript.RandSeed)));
                Debug.Assert(sizeInfo.TimeMultiplierInfo == Marshal.SizeOf(typeof(NativeScript.TimeMultiplierInfo)));
                Debug.Assert(sizeInfo.ScreenStateInfo == Marshal.SizeOf(typeof(ScreenStateInfo)));
                Debug.Assert(sizeInfo.ActionState == Marshal.SizeOf(typeof(ActionState)));
                Debug.Assert(sizeInfo.ActionElement == Marshal.SizeOf(typeof(ActionElement)));
                Debug.Assert(sizeInfo.ActionInfo == Marshal.SizeOf(typeof(ActionInfo)));
                Debug.Assert(sizeInfo.DeckEditFilterCards == Marshal.SizeOf(typeof(DeckEditFilterCards)));
                Debug.Assert(sizeInfo.CardShopOpenPackInfo == Marshal.SizeOf(typeof(CardShopOpenPackInfo)));
            }
        }

        public void RunProcessWatcher()
        {
            processWatcherRunning = true;
            new Thread(delegate()
            {
                while (processWatcherRunning)
                {
                    if (!HasProcessHandle)
                    {
                        // Give the process some time to initialization important memory
                        Thread.Sleep(2000);
                        Load();
                    }
                    Thread.Sleep(2000);
                }
            }).Start();
        }

        public void StopProcessWatcher()
        {
            processWatcherRunning = false;
        }

        public bool Load()
        {
            if (!UpdateState())
            {
                return false;
            }

            bool canLoad = false;
            while (!canLoad)
            {
                if (!UpdateState())
                {
                    return false;
                }
                switch (GetScreenState())
                {
                    case 0:
                    case ScreenState.DeveloperLogo:
                    case ScreenState.PublisherLogo:
                        break;
                    default:
                        canLoad = true;
                        break;
                }
            }

            NativeScript.Globals globals = new NativeScript.Globals();
            globals.NextDuelHandCount = -1;
            globals.CustomYdcBattlePacksEnabled = CustomYdcBattlePacksEnabled;
            globals.InitializeSeed();
            globals.UseScreenStateTransitions = UseScreenStateTransitions;
            globals.GetCurrentProcessFuncAddr = GetProcAddress("Kernel32", "GetCurrentProcess");
            globals.WriteProcessMemoryFuncAddr = GetProcAddress("Kernel32", "WriteProcessMemory");
            globals.VirtualProtectFuncAddr = GetProcAddress("Kernel32", "VirtualProtect");
            switch (Version)
            {
                case GameVersion.Lotd:
                    globals.EnterCriticalSectionFuncAddr = GetProcAddress("Kernel32", "EnterCriticalSection");
                    globals.LeaveCriticalSectionFuncAddr = GetProcAddress("Kernel32", "LeaveCriticalSection");
                    break;
                case GameVersion.LinkEvolution1:
                case GameVersion.LinkEvolution2:
                    globals.EnterCriticalSectionFuncAddr = GetProcAddress("MSVCP140", "_Mtx_lock");
                    globals.LeaveCriticalSectionFuncAddr = GetProcAddress("MSVCP140", "_Mtx_unlock");
                    break;
            }
            
            globals.srandFuncAddr = GetProcAddress("ucrtbase", "srand");
            globals.QueryPerformanceCounterFuncAddr = GetProcAddress("Kernel32", "QueryPerformanceCounter");
            globals.GetTickCount64FuncAddr = GetProcAddress("Kernel32", "GetTickCount64");
            globals.GetTickCountFuncAddr = GetProcAddress("Kernel32", "GetTickCount");
            globals.TimeGetTimeFuncAddr = GetProcAddress("winmm", "timeGetTime");
            if (globals.srandFuncAddr == IntPtr.Zero)
            {
                globals.srandFuncAddr = GetProcAddress("msvcrt", "srand");
            }
            if (!globals.IsValid || !LoadNativeScript(ref globals))
            {
                return false;
            }
            if (!IsNativeScriptLoaded)
            {
                if (!LoadNativeScript(ref globals))
                {
                    return false;
                }
                Console.WriteLine("NativeScript reloaded");
            }

            // Make sure our struct sizes match those in NativeScript
            ValidateNativeScriptStructs();

            // Set up hook func addresses (could remove this if we had a proper .obj loader)
            WriteValue(nativeScriptGlobalsAddress + NativeScript.Globals.OffsetActionHandlerHookAddress,
                GetNativeScriptFunctionAddress("ActionHandler_hook"));
            WriteValue(nativeScriptGlobalsAddress + NativeScript.Globals.OffsetAnimationHandlerHookAddress,
                GetNativeScriptFunctionAddress("AnimationHandler_hook"));
            WriteValue(nativeScriptGlobalsAddress + NativeScript.Globals.OffsetDuelInitDeckHandLPHookAddress,
                GetNativeScriptFunctionAddress("DuelInitDeckHandLP_hook"));
            WriteValue(nativeScriptGlobalsAddress + NativeScript.Globals.OffsetLoadBattlePackYdcHookAddress,
                GetNativeScriptFunctionAddress("LoadBattlePackYdc_hook"));

            // Init action hooks (for blocking certain actions / animations)
            DoAction(new ActionInfo() { Type = DoActionType.InitHooks });

            //RunTests();

            // Clear the duel loading screen wait time (make this optional?)
            SetMinimumDuelLoadingScreenTime(0);

            // Change jz to jmp to avoid window focus pausing
            if (addresses.windowFocusPauseAddress != IntPtr.Zero)
            {
                WriteValue<byte>(addresses.windowFocusPauseAddress, 0xEB);
            }

            IsFullyLoaded = true;
            if (Loaded != null)
            {
                Loaded(this, EventArgs.Empty);
            }

            return true;
        }

        /// <summary>
        /// Selects the given user deck index in the deck editor
        /// </summary>
        public void SelectUserDeck(int deckIndex)
        {
            SelectUserDeck(deckIndex, true);
        }

        /// <summary>
        /// Selects the given user deck index in the deck editor
        /// </summary>
        public void SelectUserDeck(int deckIndex, bool forceEnterDeckEditScreen)
        {
            // See deckEditApplyCardFilter.c
            IntPtr popcornCore = ReadValue<IntPtr>(addresses.popcornCoreDataAddress);
            popcornCore = ReadValue<IntPtr>(popcornCore + addresses.popcornCoreOffset);
            IntPtr deckEditBaseAddress = ReadValue<IntPtr>(popcornCore + addresses.deckEditBaseOffset);

            if (forceEnterDeckEditScreen)
            {
                // Make sure we are in the deck editor
                if (GetScreenState() != ScreenState.DeckEdit)
                {
                    // Set the return screen state for the deck editor so that the back button works correctly
                    // (otherwise the user would get stuck in the deck edit screen)
                    // See sub_1406939D0 for this +44 offset
                    WriteValue(deckEditBaseAddress + addresses.deckEditReturnScreenStateOffset, (int)ScreenState.MainMenu);

                    SetScreenState(ScreenState.DeckEdit);
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    while (true)
                    {
                        if (stopwatch.Elapsed > TimeSpan.FromSeconds(5))
                        {
                            return;
                        }                        
                        if (GetScreenState() == ScreenState.DeckEdit)
                        {
                            break;
                        }
                        System.Threading.Thread.Sleep(100);
                    }

                    // Wait a little longer for the deck editor to finish loading
                    System.Threading.Thread.Sleep(100);
                }
            }

            // See deckEditHoverUserDeck.c
            IntPtr deckEditUserDecksAddress = deckEditBaseAddress + addresses.deckEditUserDecksOffset;            
            IntPtr deckEditUserDecksScrollIndexAddress = deckEditUserDecksAddress + 8;
            // +0 has the current selected index

            int currentScrollIndex = ReadValue<byte>(deckEditUserDecksScrollIndexAddress);
            deckIndex = Math.Min(deckIndex, 31);

            // The index is relative to the current scroll index
            CallNativeScriptFunction("DeckEditSelectDeck", deckIndex - currentScrollIndex);
        }

        /// <summary>
        /// Sets the trunk cards in the deck editor
        /// </summary>
        public void SetDeckEditTrunkCards(short[] cardIds)
        {
            SetDeckEditTrunkCards(cardIds, -1);
        }

        /// <summary>
        /// Sets the trunk cards in the deck editor
        /// </summary>
        public void SetDeckEditTrunkCards(short[] cardIds, int numTotalCards)
        {
            // See deckEditApplyCardFilter.c
            IntPtr popcornCore = ReadValue<IntPtr>(addresses.popcornCoreDataAddress);
            popcornCore = ReadValue<IntPtr>(popcornCore + addresses.popcornCoreOffset);
            IntPtr deckEditBaseAddress = ReadValue<IntPtr>(popcornCore + addresses.deckEditBaseOffset);
            IntPtr deckEditTrunkAddress = deckEditBaseAddress + addresses.deckEditTrunkOffset;

            // Make sure we are in the deck editor
            if (GetScreenState() != ScreenState.DeckEdit)
            {
                // Set the return screen state for the deck editor so that the back button works correctly
                // (otherwise the user would get stuck in the deck edit screen)
                // See sub_1406939D0 for this +44 offset
                WriteValue(deckEditBaseAddress + addresses.deckEditReturnScreenStateOffset, (int)ScreenState.MainMenu);

                SetScreenState(ScreenState.DeckEdit);
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                while (true)
                {
                    if (stopwatch.Elapsed > TimeSpan.FromSeconds(5))
                    {
                        return;
                    }
                    
                    if (GetScreenState() == ScreenState.DeckEdit)
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(100);
                }

                // Wait a little longer for the deck editor to finish loading
                System.Threading.Thread.Sleep(100);
            }

            // Open the trunk UI panel
            CallNativeScriptFunction("OpenDeckEditTrunkPanel");

            if (numTotalCards < 0)
            {
                numTotalCards = ReadValue<int>(deckEditTrunkAddress + addresses.deckEditCardCountOffset);
            }

            DeckEditFilterCards filterCards = new DeckEditFilterCards();
            filterCards.NumFilteredCards = cardIds.Length;
            filterCards.NumTotalCards = numTotalCards;
            filterCards.CardIds = new short[Constants.GetNumCards2(Version)];
            Array.Copy(cardIds, filterCards.CardIds, Math.Min(filterCards.CardIds.Length, cardIds.Length));
            CallNativeScriptFunctionWithStruct("SetDeckEditTrunkCards", filterCards);

            // Manually set the card count based on the count in the card count list as we aren't
            // providing the address info for a deck used to calculate these card count values
            Manager manager = Program.Manager;
            CardState[] cardStates = ReadActualOwnedCardList();
            if (manager != null && cardStates != null)
            {
                IntPtr cardsPtr = ReadValue<IntPtr>(deckEditTrunkAddress + addresses.deckEditCardsOffset);
                for (int i = 0; i < cardIds.Length; i++)
                {
                    FileFormats.CardInfo card;
                    if (manager.CardManager.Cards.TryGetValue(cardIds[i], out card) &&
                        card.CardIndex >= 0 && card.CardIndex < cardStates.Length)
                    {
                        WriteValue(cardsPtr + 4, cardStates[card.CardIndex].Count);
                    }
                    WriteValue(cardsPtr + 8, 0);// set the number of this card in the deck
                    cardsPtr += 12;
                }
            }
        }

        /// <summary>
        /// Gets the list of cards currently visible deck editor "trunk" panel
        /// </summary>
        public short[] GetDeckEditTrunkCards()
        {
            IntPtr popcornCore = ReadValue<IntPtr>(addresses.popcornCoreDataAddress);
            popcornCore = ReadValue<IntPtr>(popcornCore + addresses.popcornCoreOffset);
            IntPtr deckEditAddress = ReadValue<IntPtr>(popcornCore + addresses.deckEditBaseOffset) + addresses.deckEditTrunkOffset;

            int numFilteredCards = ReadValue<int>(deckEditAddress + addresses.deckEditFilteredCardCountOffset);
            IntPtr cardsPtr = ReadValue<IntPtr>(deckEditAddress + addresses.deckEditCardsOffset);

            if (numFilteredCards > Constants.GetNumCards2(Version))
            {
                return new short[0];
            }

            short[] result = new short[numFilteredCards];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ReadValue<short>(cardsPtr);
                cardsPtr += 12;
            }
            return result;
        }

        /// <summary>
        /// Opens a pack with the given card ids
        /// </summary>
        public void OpenPack(short[] cardIds)
        {
            Manager manager = Program.Manager;
            CardState[] cardStates = ReadOwnedCardsList();
            if (cardStates == null || manager == null || manager.CardManager == null ||
                cardIds == null || cardIds.Length > 8)
            {
                return;
            }

            // Make sure we are in the card shop screen state
            if (GetScreenState() != ScreenState.CardShop)
            {
                SetScreenState(ScreenState.CardShop);
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                while (true)
                {
                    if (stopwatch.Elapsed > TimeSpan.FromSeconds(5))
                    {
                        return;
                    }
                    if (GetScreenState() == ScreenState.CardShop)
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(100);
                }
            }

            // Check the shop state, if it is currently opening a pack don't open another one
            if (IsOpeningPack())
            {
                return;
            }

            // This sets card ids for the visual of revealing the cards (seperate address to the actual
            // cards which are browsable once the reveal is complete)
            IntPtr shopPtr = ReadValue<IntPtr>(addresses.cardShopAddress);
            IntPtr revealedCardIdsPtrOffset = shopPtr + addresses.cardShopRevealedCardIdsOffset;
            if (ReadValue<IntPtr>(revealedCardIdsPtrOffset) == IntPtr.Zero)
            {
                // This should do a resize on the std::vector for the card shop cards address
                // - We could allocate memory manually but we would need the same allocator as the std::vector to
                //   avoid a crash when the game closes
                CallNativeScriptFunction("InitCardShopPackOpener", revealedCardIdsPtrOffset.ToInt64());
            }
            IntPtr revealedCardIdsPtr = ReadValue<IntPtr>(revealedCardIdsPtrOffset);
            short[] revealedCardIds = ReadValues<short>(revealedCardIdsPtr, 8);
            if (revealedCardIds == null)
            {
                return;
            }
            WriteValues<short>(revealedCardIdsPtr, new short[8]);
            WriteValues<short>(revealedCardIdsPtr, cardIds);

            // Set the return screen state so that the back button works correctly
            // (otherwise the user would get stuck in the card shop screen)
            // See cardShopOpenPack.c sub_1406529C0 / sub_140651D00
            WriteValue<int>(shopPtr + addresses.deckEditReturnScreenStateOffset, (int)ScreenState.MainMenu);

            // Unlock the cards
            foreach (short cardId in cardIds)
            {
                FileFormats.CardInfo card;
                if (manager.CardManager.Cards.TryGetValue(cardId, out card))
                {
                    int cardCount = Math.Min(cardStates[card.CardIndex].Count + 1, 3);
                    cardStates[card.CardIndex].Count = (byte)cardCount;
                }
            }
            WriteOwnedCardsList(cardStates, false);

            // This sets the card shop state to reveal the cards
            CardShopOpenPackInfo packInfo = new CardShopOpenPackInfo();
            packInfo.NumCards = 8;
            packInfo.CardIds = new short[packInfo.NumCards];            
            Array.Copy(cardIds, packInfo.CardIds, Math.Min(cardIds.Length, packInfo.NumCards));
            CallNativeScriptFunctionWithStruct("CardShopOpenPack", packInfo);
        }

        /// <summary>
        /// Refreshes the duel points UI in the card shop
        /// </summary>
        public void CardShopRefreshDuelPoints()
        {
            CallNativeScriptFunction("CardShopRefreshDuelPoints");
        }

        /// <summary>
        /// Returns true if a pack is currently opening in the card shop
        /// </summary>
        public bool IsOpeningPack()
        {
            IntPtr shopPtr = ReadValue<IntPtr>(addresses.cardShopAddress);
            return ReadValue<int>(shopPtr + addresses.cardShopIsOpeningPackOffset) == 3;
        }

        /// <summary>
        /// Returns true if there is an active duel or in the results screen of a duel
        /// </summary>
        public bool IsDuelScreenState()
        {
            ScreenState screenState = GetScreenState();
            return screenState == ScreenState.Duel ||
                screenState == ScreenState.DuelResult ||
                screenState == ScreenState.MatchDuelRoundResults;
        }

        /// <summary>
        /// The turn count defined by the counter in the bottom right of the screen
        /// </summary>
        public int GetTurnCount()
        {
            return ReadValue<int>(addresses.turnCountAddress) + 1;
        }

        /// <summary>
        /// The active player / whose turn it is
        /// </summary>
        public Player GetActivePlayer()
        {
            return (Player)ReadValue<int>(addresses.playerTurnAddress);
        }

        /// <summary>
        /// Reads the in-memory version of user decks defined in the save file
        /// </summary>
        public YdcDeck[] ReadUserDecks()
        {
            IntPtr saveDataAddress = addresses.GetSaveDataAddress();
            if (saveDataAddress == IntPtr.Zero)
            {
                return null;
            }
            return ReadValues<YdcDeck>(saveDataAddress + GameSaveData.GetDecksOffset(Version), Constants.NumUserDecks);
        }

        /// <summary>
        /// Writes the in-memory version of user decks defined in the save file
        /// </summary>
        public void WriteUserDecks(YdcDeck[] decks)
        {
            IntPtr saveDataAddress = addresses.GetSaveDataAddress();
            if (saveDataAddress != IntPtr.Zero)
            {
                WriteValues(saveDataAddress + GameSaveData.GetDecksOffset(Version), decks);
                MarkSaveDataDirty();
            }
        }

        /// <summary>
        /// Hides the default cards given to the player based on the default structure deck info (currently no dlc support)
        /// </summary>
        public void HideDefaultStructureDeckCards()
        {
            // Sets the first default structure deck index to 0 which will hide the default cards from those decks
            // in the deck editor (the actual decks should still be visible in the deck editor / elsewhere)
            WriteValue<int>(addresses.defaultStructureDeckCardsAddress, 0);
        }

        /// <summary>
        /// Reads the owned cards visible in the deck editor, some of which is obtained from default structure decks
        /// - Will be empty if you don't open the deck editor
        /// - Not super useful - making this a private method for now
        /// </summary>
        private CardState[] ReadDeckEditorOwnedCardsList()
        {
            IntPtr saveDataAddress = addresses.GetDeckEditorOwnedCardListAddress();
            if (saveDataAddress == IntPtr.Zero)
            {
                return null;
            }
            return ReadValues<CardState>(saveDataAddress, Constants.GetNumCards2(Version));
        }

        /// <summary>
        /// Reads the owned card list + default owned card list and combines them
        /// </summary>
        public CardState[] ReadActualOwnedCardList()
        {
            CardState[] defaultOwnedCards = ReadDefaultOwnedCardsList();
            CardState[] ownedCards = ReadOwnedCardsList();
            CardState[] result = new CardState[Constants.GetNumCards2(Version)];
            if (ownedCards != null)
            {
                for (int i = 0; i < ownedCards.Length && i < result.Length; i++)
                {
                    result[i].RawValue = ownedCards[i].RawValue;
                }
            }
            if (defaultOwnedCards != null)
            {
                for (int i = 0; i < defaultOwnedCards.Length && i < result.Length; i++)
                {
                    int defaultCardCount = defaultOwnedCards[i].Count;                    
                    if (defaultCardCount > 0)
                    {
                        int cardCount = Math.Min(result[i].Count + defaultCardCount, 3);
                        result[i].Count = (byte)cardCount;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Attempts to get the default owned cards list from default structure decks (currently not handling dlc structure decks)
        /// </summary>
        /// <returns></returns>
        public CardState[] ReadDefaultOwnedCardsList()
        {
            CardState[] result = new CardState[Constants.GetNumCards2(Version)];
            YdcDeck[] decks = ReadYdcDecks();
            Manager manager = Program.Manager;
            if (decks != null && manager != null && manager.CardManager != null)
            {
                for (int i = 0; i < int.MaxValue; i++)
                {
                    int deckOffset = ReadValue<int>(addresses.defaultStructureDeckCardsAddress + (i * 4));
                    if (deckOffset <= 0)
                    {
                        break;
                    }
                    if (deckOffset < decks.Length)
                    {
                        YdcDeck deck = decks[deckOffset];
                        List<short> cardIds = new List<short>();

                        for (int j = 0; j < deck.NumMainDeckCards; j++)
                        {
                            cardIds.Add(deck.MainDeck[j]);
                        }
                        for (int j = 0; j < deck.NumSideDeckCards; j++)
                        {
                            cardIds.Add(deck.SideDeck[j]);
                        }
                        for (int j = 0; j < deck.NumSideDeckCards; j++)
                        {
                            cardIds.Add(deck.ExtraDeck[j]);
                        }

                        foreach(short cardId in cardIds)
                        {
                            if (cardId > 0 && cardId <= Constants.GetMaxCardId(Version))
                            {
                                FileFormats.CardInfo card;
                                if (manager.CardManager.Cards.TryGetValue(cardId, out card))
                                {
                                    int cardCount = Math.Min(result[card.CardIndex].Count + 1, 3);
                                    result[card.CardIndex].Count = (byte)cardCount;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Reads the in-memory version of the user owned card list defined in the save file
        /// </summary>
        public CardState[] ReadOwnedCardsList()
        {
            IntPtr saveDataAddress = addresses.GetSaveDataAddress();
            if (saveDataAddress == IntPtr.Zero)
            {
                return null;
            }
            return ReadValues<CardState>(saveDataAddress + GameSaveData.GetCardListOffset(Version), Constants.GetNumCards2(Version));
        }

        /// <summary>
        /// Writes the in-memory version of the user owned card list defined in the save file
        /// </summary>
        public void WriteOwnedCardsList(CardState[] cards)
        {
            WriteOwnedCardsList(cards, true);
        }

        /// <summary>
        /// Writes the in-memory version of the user owned card list defined in the save file
        /// </summary>
        public void WriteOwnedCardsList(CardState[] cards, bool save)
        {
            IntPtr saveDataAddress = addresses.GetSaveDataAddress();
            if (saveDataAddress != IntPtr.Zero)
            {
                WriteValues(saveDataAddress + GameSaveData.GetCardListOffset(Version), cards);
                if (save)
                {
                    MarkSaveDataDirty();
                }
            }
        }

        /// <summary>
        /// Sets the given owned card count number on all cards
        /// </summary>
        /// <param name="cardCount"></param>
        public void SetAllOwnedCardsCount(byte cardCount)
        {
            CardState[] cards = ReadOwnedCardsList();
            if (cards != null && cards.Length > 0)
            {
                for (int i = 0; i < cards.Length; i++)
                {
                    if (cardCount == 0)
                    {
                        cards[i].Seen = false;
                    }
                    cards[i].Count = cardCount;
                }
                WriteOwnedCardsList(cards);
            }
        }

        /// <summary>
        /// Unlocks all cards (calls SetAllOwnedCardsCount(3))
        /// </summary>
        public void UnlockAllCards()
        {
            SetAllOwnedCardsCount(3);
        }

        /// <summary>
        /// Gets the address of the in-memory version of recipes save data
        /// </summary>
        private IntPtr GetSaveDataRecipesAddress()
        {
            IntPtr saveDataAddress = addresses.GetSaveDataAddress();
            if (saveDataAddress != IntPtr.Zero)
            {
                saveDataAddress += GameSaveData.GetMiscDataOffset(Version) + 16 + 8 + 32 + (4 * Constants.GetNumDeckDataSlots(Version));
            }
            return saveDataAddress;
        }

        /// <summary>
        /// Reads the in-memory version of the unlocked recipes defined in the save file
        /// </summary>
        public bool[] ReadUnlockedRecipes()
        {
            bool[] result = new bool[Constants.GetNumDeckDataSlots(Version)];

            IntPtr recipesAddress = GetSaveDataRecipesAddress();
            if (recipesAddress != IntPtr.Zero)
            {
                byte[] unlockedRecipesBuffer = ReadBytes(recipesAddress, MiscSaveData.GetRecipeBufferBytes(Version));
                for (int i = 0; i < Constants.GetNumDeckDataSlots(Version); i++)
                {
                    int byteIndex = i / 8;
                    int bitIndex = i % 8;
                    result[i] = (unlockedRecipesBuffer[byteIndex] & (byte)(1 << bitIndex)) != 0;
                }
            }

            return result;
        }

        /// <summary>
        /// Writes the in-memory version of the unlocked recipes defined in the save file
        /// </summary>
        public void WriteUnlockedRecipes(bool[] recipes)
        {
            IntPtr recipesAddress = GetSaveDataRecipesAddress();
            if (recipesAddress != IntPtr.Zero)
            {
                byte[] recipesData = new byte[MiscSaveData.GetRecipeBufferBytes(Version)];
                for (int i = 0; i < recipes.Length && i < Constants.GetNumDeckDataSlots(Version); i++)
                {
                    int byteIndex = i / 8;
                    int bitIndex = i % 8;
                    if (recipes[i])
                    {
                        recipesData[byteIndex] |= (byte)((1 << bitIndex));
                    }
                }
                WriteBytes(recipesAddress, recipesData);
            }
        }

        /// <summary>
        /// Sets the in-memory version of recipes to the given complete state
        /// </summary>
        private void SetAllRecipesComplete(bool complete)
        {
            bool[] recipes = new bool[Constants.GetNumDeckDataSlots(Version)];
            for (int i = 0; i < recipes.Length; i++)
            {
                recipes[i] = complete;
            }
            WriteUnlockedRecipes(recipes);
        }

        /// <summary>
        /// Sets all recipes to unlocked / available
        /// </summary>
        public void UnlockAllRecipes()
        {
            SetAllRecipesComplete(true);
        }

        /// <summary>
        /// Sets all recipes to locked / unavailable
        /// </summary>
        public void LockAllRecipes()
        {
            SetAllRecipesComplete(false);
        }

        /// <summary>
        /// Gets the address of the in-memory version of duel points save data
        /// </summary>
        private IntPtr GetDuelPointsAddress()
        {
            IntPtr saveDataAddress = addresses.GetSaveDataAddress();
            if (saveDataAddress != IntPtr.Zero)
            {
                saveDataAddress += GameSaveData.GetMiscDataOffset(Version) + 16;
            }
            return saveDataAddress;
        }

        /// <summary>
        /// Gets the duel points
        /// </summary>
        public int GetDuelPoints()
        {
            IntPtr duelPointsAddress = GetDuelPointsAddress();
            if (duelPointsAddress != IntPtr.Zero)
            {
                return ReadValue<int>(duelPointsAddress);
            }
            return 0;
        }

        /// <summary>
        /// Sets the duel points
        /// </summary>
        public void SetDuelPoints(int duelPoints)
        {
            IntPtr duelPointsAddress = GetDuelPointsAddress();
            if (duelPointsAddress != IntPtr.Zero)
            {
                WriteValue(duelPointsAddress, duelPoints);
            }
        }

        /// <summary>
        /// Reads the in-memory version of the save file data
        /// </summary>
        public byte[] ReadSaveData()
        {
            IntPtr saveDataAddress = addresses.GetSaveDataAddress();
            if (saveDataAddress != IntPtr.Zero)
            {
                byte[] buffer = new byte[GameSaveData.GetFileLength(Version)];
                IntPtr readBytes;
                ReadProcessMemoryEx(ProcessHandle, saveDataAddress, buffer, (IntPtr)buffer.Length, out readBytes);
                return buffer;
            }
            return null;
        }

        /// <summary>
        /// Writes the in-memory version of the save file data
        /// </summary>
        public void WriteSaveData(byte[] saveData)
        {
            Debug.Assert(saveData.Length <= GameSaveData.GetFileLength(Version));

            IntPtr saveDataAddress = addresses.GetSaveDataAddress();
            if (saveDataAddress != IntPtr.Zero)
            {
                IntPtr writtenBytes;
                WriteProcessMemoryEx(ProcessHandle, saveDataAddress, saveData, (IntPtr)saveData.Length, out writtenBytes);
            }
            MarkSaveDataDirty();
        }

        /// <summary>
        /// Writes the given chunk to the in-memory version of the save file data
        /// </summary>
        public void SetSaveDataChunk(byte[] chunk, int chunkOffset)
        {
            if (!UpdateState())
            {
                return;
            }

            if (chunk != null && chunk.Length <= GameSaveData.GetFileLength(Version) && chunk.Length >= 0 && chunkOffset <= GameSaveData.GetFileLength(Version))
            {
                IntPtr saveDataAddress = addresses.GetSaveDataAddress();
                if (saveDataAddress != IntPtr.Zero)
                {
                    IntPtr writtenBytes;
                    WriteProcessMemoryEx(ProcessHandle, saveDataAddress + chunkOffset, chunk, (IntPtr)chunk.Length, out writtenBytes);
                    MarkSaveDataDirty();
                }
            }
        }

        /// <summary>
        /// Marks the save data as dirty (save the save file with whats in memory)
        /// </summary>
        public void MarkSaveDataDirty()
        {
            // This comes from sub_14061F470 - see savegame.c
            IntPtr address = ReadValue<IntPtr>(addresses.saveDataAddress2);
            if (address != IntPtr.Zero)
            {
                WriteValue<byte>(address + addresses.dirtySaveDataOffset1, 0);
                WriteValue<int>(address + addresses.dirtySaveDataOffset2, 1);
                WriteValue<int>(address + addresses.dirtySaveDataOffset3, 2);
            }
        }

        /// <summary>
        /// Gets the current screen state
        /// </summary>
        public ScreenState GetScreenState()
        {
            // This comes from sub_14062CFF0 - see screenstate.c
            IntPtr popcornCore = ReadValue<IntPtr>(addresses.popcornCoreDataAddress);
            popcornCore = ReadValue<IntPtr>(popcornCore + addresses.popcornCoreOffset);
            long v1 = ReadValue<long>(popcornCore + addresses.screenStateOffset1);
            int v2 = ReadValue<int>(popcornCore + addresses.screenStateOffset2);
            int screenState = ReadValue<int>(v1 + addresses.screenStateOffset3 * v2);
            return (ScreenState)screenState;
        }

        /// <summary>
        /// Sets the current screen state
        /// </summary>
        public void SetScreenState(int screenState)
        {
            SetScreenState((ScreenState)screenState);
        }

        /// <summary>
        /// Sets the current screen state
        /// </summary>
        public void SetScreenState(ScreenState screenState)
        {
            // Default transition time in seconds
            double transitionTime = 0.15;
            if (!UseScreenStateTransitions)
            {
                transitionTime = 0;
            }
            SetScreenState(screenState, ScreenTransitionType.Default, transitionTime);
        }

        /// <summary>
        /// Sets the current screen state
        /// </summary>
        public void SetScreenState(ScreenState screenState, ScreenTransitionType transitionType, double transitionTime)
        {
            SetScreenState((int)screenState, (int)transitionType, transitionTime);
        }

        /// <summary>
        /// Sets the current screen state
        /// </summary>
        public void SetScreenState(int screenState, int transitionType, double transitionTime)
        {
            ScreenStateInfo screenStateInfo = new ScreenStateInfo();
            screenStateInfo.State = screenState;
            screenStateInfo.TransitionType = transitionType;
            screenStateInfo.TransitionTime = transitionTime;
            CallNativeScriptFunctionWithStruct("SetScreenState", screenStateInfo);
        }

        /// <summary>
        /// Sets the hand size limit
        /// </summary>
        public void SetHandSizeLimit(int handSizeLimit)
        {
            WriteValue<int>(addresses.handSizeLimitAddress, handSizeLimit);
        }

        /// <summary>
        /// Resets the hand size limit to the default value (6)
        /// </summary>
        public void ResetHandSizeLimit()
        {
            SetHandSizeLimit(6);
        }

        /// <summary>
        /// Starts a custom made duel
        /// </summary>
        public void StartDuel(StartDuelInfo duelInfo)
        {
            // Be careful with data types for WriteValue (or just be explicit by always using WriteValue<XXX>)

            if (IsOpeningPack())
            {
                return;
            }

            if (duelInfo.FullReload)
            {
                // Set tutorial duel mode, this will be cleared when the main menu is hit
                WriteValue<byte>(addresses.modeTutorialDuelAddress, 1);

                SetScreenState(ScreenState.MainMenu);
                ScreenState screenState = GetScreenState();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                while (true)
                {
                    if (stopwatch.Elapsed > TimeSpan.FromSeconds(5))
                    {
                        return;
                    }

                    // Wait for screen state to leave duel and for our tutorial duel mode to be cleared
                    screenState = GetScreenState();
                    if (ReadValue<byte>(addresses.modeTutorialDuelAddress) == 0 &&
                        screenState != ScreenState.Duel &&
                        screenState != ScreenState.DuelLoadingScreen &&
                        screenState != ScreenState.DuelResult)
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(100);
                }
                // For good measure as some things may not be fully cleared yet
                System.Threading.Thread.Sleep(300);
            }

            int duelSeed = duelInfo.RandSeed != 0 ? duelInfo.RandSeed : rand.Next();
            WriteValue<int>(nativeScriptGlobalsAddress + NativeScript.Globals.OffsetDuelSeed, duelSeed);

            WriteValue<int>(nativeScriptGlobalsAddress + NativeScript.Globals.OffsetNextDuelHandCount, duelInfo.StartingHandCount);

            if (duelInfo.SpeedDuel)
            {
                WriteValue<int>(nativeScriptGlobalsAddress + NativeScript.Globals.OffsetIsNextDuelSpeedDuel, 1);
            }
            else if (duelInfo.RushDuel)
            {
                WriteValue<int>(nativeScriptGlobalsAddress + NativeScript.Globals.OffsetIsNextDuelSpeedDuel, 2);
            }

            if (duelInfo.SpeedDuel && duelInfo.UseSpeedDuelLifePoints)
            {
                // Speed duel life points are cleared by the default starting life points
                // Set the starting life points to speed duel life points (if enabled)
                duelInfo.LifePoints = Constants.DefaultLifePoints / 2;
            }

            WriteValue<int>(addresses.modeTurnTimeLimitEnabledAddress, duelInfo.TurnTimeLimitEnabled ? 1 : 0);
            WriteValue<long>(addresses.modeTurnTimeLimitAddress, duelInfo.TurnTimeLimit);

            WriteValue<byte>(addresses.modeAIModeAddress, (byte)duelInfo.AIMode);
            WriteValue<int>(addresses.modeStartingLifePointsAddress, duelInfo.LifePoints);
            WriteValue<byte>(addresses.modeMatchDuelAddress, (byte)(duelInfo.Match ? 1 : 0));
            WriteValue<byte>(addresses.modeTagDuelAddress, (byte)(duelInfo.TagDuel ? 1 : 0));
            WriteValue<byte>(addresses.modeInstantStartDuelAddress, (byte)(duelInfo.SkipRockPaperScissors ? 1 : 0));
            WriteValue<byte>(addresses.modeTestOptionAddress, (byte)duelInfo.TestOption);//long/qword?

            bool tutorialDuel = false;
            bool campaignDuel = false;
            bool battlePackDuel = false;
            bool challengeDuel = false;
            switch (duelInfo.DuelType)
            {
                case DuelType.Tutorial:
                    tutorialDuel = true;
                    break;
                case DuelType.Campaign:
                    campaignDuel = true;
                    break;
                case DuelType.BattlePack:
                    battlePackDuel = true;
                    break;
                case DuelType.Challenge:
                    challengeDuel = true;
                    break;
            }
            WriteValue<byte>(addresses.modeTutorialDuelAddress, (byte)(tutorialDuel ? 1 : 0));
            WriteValue<byte>(addresses.modeTutorialDuelIndexAddress, (byte)duelInfo.TutorialDuelIndex);

            WriteValue<byte>(addresses.modeCampaignDuelAddress, (byte)(campaignDuel ? 1 : 0));
            WriteValue<int>(addresses.modeCampaignDuelIndexAddress, duelInfo.CampaignDuelIndex);
            WriteValue<int>(addresses.modeCampaignDuelDeckIndexAddress, duelInfo.CampaignDuelDeckIndex);

            WriteValue<byte>(addresses.modeBattlePackDuelAddress, (byte)(battlePackDuel ? 1 : 0));
            WriteValue<int>(addresses.modeBattlePackDuelAddress, duelInfo.BattlePackIndex);

            WriteValue<byte>(addresses.modeChallengeDuelAddress, (byte)(challengeDuel ? 1 : 0));

            if (addresses.modeMasterRules != IntPtr.Zero)
            {
                WriteValue<byte>(addresses.modeMasterRules, (byte)(duelInfo.MasterRules5 ? 1 : 0));
            }

            // Set the controllers for each player (AI / player)
            WriteValue<int>(addresses.modePlayerControllerAddress + (4 * GetPlayerIndex(Player.Self)), (int)duelInfo.GetController(Player.Self));
            WriteValue<int>(addresses.modePlayerControllerAddress + (4 * GetPlayerIndex(Player.Opponent)), (int)duelInfo.GetController(Player.Opponent));
            WriteValue<int>(addresses.modePlayerControllerAddress + (4 * GetPlayerIndex(Player.TagSelf)), (int)duelInfo.GetController(Player.TagSelf));
            WriteValue<int>(addresses.modePlayerControllerAddress + (4 * GetPlayerIndex(Player.TagOpponent)), (int)duelInfo.GetController(Player.TagOpponent));

            // Set the player deck ids (player ids are 0-32, ydc are 33+)
            WriteValue(addresses.playerDeckIdAddress + (4 * GetPlayerIndex(Player.Self)), duelInfo.GetDeckId(Player.Self));
            WriteValue(addresses.playerDeckIdAddress + (4 * GetPlayerIndex(Player.Opponent)), duelInfo.GetDeckId(Player.Opponent));
            WriteValue(addresses.playerDeckIdAddress + (4 * GetPlayerIndex(Player.TagSelf)), duelInfo.GetDeckId(Player.TagSelf));
            WriteValue(addresses.playerDeckIdAddress + (4 * GetPlayerIndex(Player.TagOpponent)), duelInfo.GetDeckId(Player.TagOpponent));

            // Set the player avatar ids
            WriteValue(addresses.playerAvatarIdAddress + (4 * GetPlayerIndex(Player.Self)), duelInfo.GetAvatarId(Player.Self));
            WriteValue(addresses.playerAvatarIdAddress + (4 * GetPlayerIndex(Player.Opponent)), duelInfo.GetAvatarId(Player.Opponent));
            WriteValue(addresses.playerAvatarIdAddress + (4 * GetPlayerIndex(Player.TagSelf)), duelInfo.GetAvatarId(Player.TagSelf));
            WriteValue(addresses.playerAvatarIdAddress + (4 * GetPlayerIndex(Player.TagOpponent)), duelInfo.GetAvatarId(Player.TagOpponent));

            // Set the duel arena id
            WriteValue(addresses.duelArenaAddress, (int)duelInfo.Arena);

            // Set the starting player for the duel
            WriteValue(addresses.modeStartingPlayerAddress, GetPlayerIndex(duelInfo.StartingPlayer));

            if (CallNativeScriptFunction("LoadDuel") == CallNativeFunctionResult.Success)
            {
                // Some settings must be written after loading the duel as they are reset in the LoadDuel call

                WriteValue<byte>(addresses.modeAIModeAddress, (byte)duelInfo.AIMode);

                // Set the screen state to the duel screen state (starts the duel / rock paper scissors)
                SetScreenState(ScreenState.Duel);
            }
            else
            {
                // Clear these values as otherwise they may be used on an undesired duel
                WriteValue<int>(nativeScriptGlobalsAddress + NativeScript.Globals.OffsetNextDuelHandCount, -1);
                WriteValue<int>(nativeScriptGlobalsAddress + NativeScript.Globals.OffsetIsNextDuelSpeedDuel, 0);
            }
        }

        /// <summary>
        /// Gets the currently active dueling arena / background environment
        /// </summary>
        public DuelArena GetDuelArena()
        {
            return (DuelArena)ReadValue<int>(addresses.duelArenaAddress);
        }

        /// <summary>
        /// Gets the card id which is currently under the cursor (the card seen in the details on the left side of the screen)
        /// </summary>
        public short GetHoveredCardId()
        {
            return GetHoveredCardId(false);
        }

        /// <summary>
        /// Gets the card id which is currently under the cursor (the card seen in the details on the left side of the screen)
        /// </summary>
        public short GetHoveredCardId(bool includeHiddenCards)
        {
            int playerIndex, slotIndex, slotSubIndex, cardId;
            GetHoveredSlotInfo(out playerIndex, out slotIndex, out slotSubIndex, out cardId, includeHiddenCards);
            return (short)cardId;
        }

        /// <summary>
        /// Gets slot info under the cursor
        /// </summary>
        private void GetHoveredSlotInfo(out int playerIndex, out int slotIndex, out int slotSubIndex, out int cardId, bool includeHiddenCards)
        {
            IntPtr coreDuelData = ReadValue<IntPtr>(addresses.coreDuelDataAddress);

            // See "YGOPopcornCore_cardhover.c" sub_140876690 / sub_14089BFC0  for these offsets
            int offset = addresses.hoveredCardOffset1 * ReadValue<byte>(coreDuelData + addresses.hoveredCardOffset2);
            IntPtr cursorCardDataPtr = (coreDuelData + addresses.hoveredCardOffset3) + offset + addresses.hoveredCardOffset4;

            cardId = ReadValue<short>(cursorCardDataPtr);

            ushort slotInfo = ReadValue<ushort>(cursorCardDataPtr + 240);// TODO: UPDATE!
            playerIndex = slotInfo & 1;
            slotIndex = (slotInfo >> 1) & 0x1F;
            slotSubIndex = slotInfo >> 6;

            if (cardId > 0 || !includeHiddenCards)
            {
                return;
            }

            DuelPlayerInfo playerInfo = ReadPlayerInfo((Player)playerIndex);

            if (slotIndex >= 0 && slotIndex <= 12)
            {
                // normal field slots
                cardId = playerInfo.Field[slotIndex].Card.CardId;
            }
            else
            {
                switch ((SlotType)slotIndex)
                {
                    case SlotType.Hand:
                        if (slotSubIndex < playerInfo.NumCardsInHand)
                        {
                            cardId = playerInfo.Hand[slotSubIndex].CardId;
                        }
                        break;
                    case SlotType.ExtraDeck:
                        if (slotSubIndex < playerInfo.NumCardsInExtraDeck)
                        {
                            cardId = playerInfo.ExtraDeck[slotSubIndex].CardId;
                        }
                        break;
                    case SlotType.Deck:
                        if (slotSubIndex < playerInfo.NumCardsInDeck)
                        {
                            cardId = playerInfo.Deck[slotSubIndex].CardId;
                        }
                        break;
                    case SlotType.Graveyard:
                        if (slotSubIndex < playerInfo.NumCardsInGraveyard)
                        {
                            cardId = playerInfo.Graveyard[slotSubIndex].CardId;
                        }
                        break;
                    case SlotType.Banished:
                        if (slotSubIndex < playerInfo.NumCardsBanished)
                        {
                            cardId = playerInfo.Banished[slotSubIndex].CardId;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Draws a card for the target player
        /// </summary>
        public void DrawCard(Player target)
        {
            DrawCards(target, 1);
        }

        /// <summary>
        /// Draws the given number of cards for the target player
        /// </summary>
        public void DrawCards(Player target, int count)
        {
            QueueAction(new ActionInfo()
            {
                ActionId = EncodeActionId(target, ActionId.DrawCard),
                ActionData1 = 0xFFFF,// 0xFFFF defines if its a starting draw?
                ActionData2 = (ushort)count,// Number of cards to draw
            });
        }

        /// <summary>
        /// Draws a card without waiting for the last card draw to complete (or any other action)
        /// </summary>
        public void ForceDrawCard(Player target)
        {
            ForceDrawCards(target, 1);
        }

        /// <summary>
        /// Draws the given number of cards without waiting for the last card draw to complete (or any other action)
        /// </summary>
        public void ForceDrawCards(Player target, int count)
        {
            // State:
            // 0-0/1-0/0-2/2-0 = draw nothing
            // 0-1 = draw 1 card
            // 1-1/2-1/1-2/2-2 = draw cards forever

            ForceAction(new ActionInfo()
            {
                SetState = true,
                State1 = 0,
                State2 = 1,
                ActionId = EncodeActionId(target, ActionId.DrawCard),
                ActionData1 = 0xFFFF,
                ActionData2 = 1
            }, count);
        }

        /// <summary>
        /// Set the life points of the given player
        /// </summary>
        public void SetLifePoints(Player target, ushort lifePoints)
        {
            QueueAction(new ActionInfo()
            {
                ActionId = EncodeActionId(target, ActionId.SetLifePoints),
                ActionData1 = lifePoints,
                ActionData2 = lifePoints,
                //ActionData3 = 1// targets both players and contributes to an achievement
            });
        }

        /// <summary>
        /// Plays the given audio snippet
        /// </summary>
        public bool PlayAudio(AudioSnippet audio)
        {
            return PlayAudio((int)audio);
        }

        /// <summary>
        /// Plays the given audio snippet
        /// </summary>
        public bool PlayAudio(int audioIndex)
        {
            return CallNativeScriptFunction("PlayAudio", audioIndex) == CallNativeFunctionResult.Success;
        }

        /// <summary>
        /// Stops the given audio snippet
        /// </summary>
        public bool StopAudio(AudioSnippet audio)
        {
            return StopAudio((int)audio);
        }

        /// <summary>
        /// Stops the given audio snippet
        /// </summary>
        public bool StopAudio(int audioIndex)
        {
            return CallNativeScriptFunction("StopAudio", audioIndex) == CallNativeFunctionResult.Success;
        }

        /// <summary>
        /// Stops all audio which is currently playing
        /// </summary>
        public bool StopAllAudio()
        {
            return CallNativeScriptFunction("StopAllAudio") == CallNativeFunctionResult.Success;
        }

        /// <summary>
        /// Sets the minimum duel loadding screen time in seconds
        /// </summary>
        public void SetMinimumDuelLoadingScreenTime(double seconds)
        {
            WriteValue(addresses.duelLoadingScreenDelayAddress, seconds);
        }

        /// <summary>
        /// Reads the active duel information for the given player (life points, hand, field, deck, etc)
        /// </summary>
        public DuelPlayerInfo ReadPlayerInfo(Player player)
        {
            IntPtr address = addresses.duelPlayerInfoAddress + (GetPlayerIndex(player) * Marshal.SizeOf(typeof(DuelPlayerInfo)));
            return ReadValue<DuelPlayerInfo>(address);
        }

        /// <summary>
        /// Writes the active duel information for the given player (life points, hand, field, deck, etc)
        /// </summary>
        public void WritePlayerInfo(Player playerTarget, DuelPlayerInfo player)
        {
            IntPtr address = addresses.duelPlayerInfoAddress + (GetPlayerIndex(playerTarget) * Marshal.SizeOf(typeof(DuelPlayerInfo)));
            WriteValue(address, player);
        }

        /// <summary>
        /// Reads the custom battle pack .ydc decks (AI decks used in battle pack mode)
        /// </summary>
        public YdcDeck[] ReadCustomBattlePackYdcDecks()
        {
            return ReadValues<YdcDeck>(nativeScriptGlobalsAddress + NativeScript.Globals.OffsetBattlePackDecks,
                NativeScript.Globals.NumCustomBattlePackDecks);
        }

        /// <summary>
        /// Writes the custom battle pack .ydc decks (AI decks used in battle pack mode)
        /// </summary>
        public void WriteCustomBattlePackYdcDecks(YdcDeck[] decks)
        {
            WriteValues(nativeScriptGlobalsAddress + NativeScript.Globals.OffsetBattlePackDecks, decks);
        }

        /// <summary>
        /// Reads the in-memory version of .ydc decks
        /// </summary>
        public YdcDeck[] ReadYdcDecks()
        {
            YdcDeck[] decks = ReadValues<YdcDeck>(addresses.ydcDecksAddress, Constants.GetNumDeckDataSlots(Version));
            CheckDecksForUnknownData(decks);
            return decks;
        }

        /// <summary>
        /// Writes the in-memory version of .ydc decks
        /// </summary>
        public void WriteYdcDecks(YdcDeck[] decks)
        {
            WriteValues(addresses.ydcDecksAddress, decks);
        }

        /// <summary>
        /// Validate some in-memory ydc deck unknowns are all zero bytes
        /// </summary>
        private void CheckDecksForUnknownData(YdcDeck[] decks)
        {
            if (decks != null)
            {
                foreach (YdcDeck deck in decks)
                {
                    //Debug.Assert(Array.TrueForAll(deck.Unk1, x => x == 0));
                    //Debug.Assert(Array.TrueForAll(deck.Unk2, x => x == 0));
                    Debug.Assert(deck.Unk3 == 0);
                    Debug.Assert(deck.Unk4 == 0);
                    Debug.Assert(deck.Unk5 == 0);
                    Debug.Assert(deck.Unk6 == 0);
                    Debug.Assert(deck.Unk7 == 0);
                }
            }
        }

        /// <summary>
        /// Reads the in-memory version of card props (each index is a card id)
        /// </summary>
        /// <returns></returns>
        public CardProps[] ReadCardProps()
        {
            return ReadValues<CardProps>(addresses.cardPropsBinAddress, Constants.GetMaxCardId(Version) + 1);
        }

        /// <summary>
        /// Writes the the in-memory of card props (each index is a card id)
        /// </summary>
        public void WriteCardProps(CardProps[] cardProps)
        {
            Debug.Assert(cardProps.Length <= Constants.GetMaxCardId(Version) + 1);
            WriteValues(addresses.cardPropsBinAddress, cardProps);
        }

        /// <summary>
        /// Reads the in-memory version of battle pack data
        /// </summary>
        /// <param name="packName">The name of the pack after the first underscore e.g. "battlepack1" for "bpack_BattlePack1.bin"</param>
        /// <returns></returns>
        public CardCollection[] ReadBattlePackData(string packName)
        {
            packName = packName.ToLower();

            List<CardCollection> result = new List<CardCollection>();
            RawPackDefData[] dataArray = ReadValues<RawPackDefData>(addresses.packDefDataBinAddress, 128);
            for (int i = 0; i < dataArray.Length; i++)
            {
                RawPackDefData data = dataArray[i];
                if (ReadStringASCII(data.CodeName).Equals(packName, StringComparison.InvariantCultureIgnoreCase))
                {
                    IntPtr ptr = data.BattlePackData;
                    long numCategories = ReadValue<long>(ptr);
                    ptr += 8;
                    for (int j = 0; j < numCategories; j++)
                    {
                        IntPtr cardListOffset = ReadValue<IntPtr>(ptr);
                        ptr += 8;

                        IntPtr tempPtr = ptr;

                        ptr = cardListOffset;
                        ushort cardCount = ReadValue<ushort>(ptr);
                        ptr += 2;
                        CardCollection cardCollection = new CardCollection();
                        for (int k = 0; k < cardCount; k++)
                        {
                            cardCollection.Add(ReadValue<short>(ptr));
                            ptr += 2;
                        }
                        result.Add(cardCollection);

                        ptr = tempPtr;
                    }
                    break;
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Writes the in-memory version of battle pack data. The game will hang if you don't fill this out properly.
        /// </summary>
        /// <param name="packName">The name of the pack after the first underscore e.g. "battlepack1" for "bpack_BattlePack1.bin"</param>
        public void WriteBattlePackData(string packName, CardCollection[] categories)
        {
            packName = packName.ToLower();

            int expectedLen = 8;// category count
            for (int i = 0; i < categories.Length; i++)
            {
                expectedLen += 8 + 2;// offset + card count
                expectedLen += categories[i].CardIds.Count * 2;
            }

            if (expectedLen > customBattlePackMaxDataLen)
            {
                throw new Exception("Battle pack too large. Size: " + expectedLen + " limit: " + customBattlePackMaxDataLen);
            }

            RawPackDefData[] dataArray = ReadValues<RawPackDefData>(addresses.packDefDataBinAddress, 128);
            for (int i = 0; i < dataArray.Length; i++)
            {
                RawPackDefData data = dataArray[i];
                if (ReadStringASCII(data.CodeName).Equals(packName, StringComparison.InvariantCultureIgnoreCase))
                {
                    byte[] buffer = new byte[customBattlePackMaxDataLen];

                    IntPtr address;
                    if (!customBattlePackDataAddresses.TryGetValue(packName, out address))
                    {
                        address = AllocateRemoteBuffer(buffer);
                        if (address == IntPtr.Zero)
                        {
                            return;
                        }
                        customBattlePackDataAddresses.Add(packName, address);
                    }

                    // Clear the existing data
                    WriteBytes(address, buffer);

                    IntPtr ptr = address;
                    WriteValue<long>(ptr, categories.Length);
                    ptr += 8;

                    IntPtr offsetsBasePtr = ptr;
                    // Move to the where the category contents will go
                    ptr += categories.Length * 8;

                    for (int j = 0; j < categories.Length; j++)
                    {
                        IntPtr tempPtr = ptr;

                        // Write the offset
                        ptr = offsetsBasePtr + (j * 8);
                        WriteValue<IntPtr>(ptr, tempPtr);
                        ptr = tempPtr;

                        // Write the category data
                        WriteValue<short>(ptr, (short)categories[j].CardIds.Count);
                        ptr += 2;
                        foreach (short cardId in categories[j].CardIds)
                        {
                            WriteValue<short>(ptr, cardId);
                            ptr += 2;
                        }
                    }

                    data.BattlePackData = address;
                    WriteValue(addresses.packDefDataBinAddress + (i * Marshal.SizeOf(typeof(RawPackDefData))), data);
                }
            }
        }

        /// <summary>
        /// Gets an array of available short-form battle pack names
        /// </summary>
        public string[] GetBattlePackNames()
        {
            List<string> result = new List<string>();
            RawPackDefData[] dataArray = ReadValues<RawPackDefData>(addresses.packDefDataBinAddress, 128);
            for (int i = 0; i < dataArray.Length; i++)
            {
                RawPackDefData data = dataArray[i];
                if ((FileFormats.PackType)data.PackType == FileFormats.PackType.Battle)
                {
                    result.Add(ReadStringASCII(data.CodeName));
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Reads the in-memory version of CARD_Props.bin
        /// </summary>
        public FileFormats.CardGenre[] ReadCardGenres()
        {
            long dataLen = ReadValue<long>(addresses.cardGenreBinAddress);
            IntPtr dataAddress = ReadValue<IntPtr>(addresses.cardGenreBinAddress + 8);

            // Should be 8 bytes for each card
            Debug.Assert(dataLen == Constants.GetNumCards(Version) * 8);

            FileFormats.CardGenre[] cardGenres = new FileFormats.CardGenre[Constants.GetNumCards(Version)];
            for (int i = 0; i < cardGenres.Length; i++)
            {
                cardGenres[i] = (FileFormats.CardGenre)ReadValue<ulong>(dataAddress + (i * 8));
            }

            return cardGenres;
        }

        /// <summary>
        /// Writes the in-memory version of CARD_Props.bin
        /// </summary>
        public void WriteCardGenres(FileFormats.CardGenre[] cardGenres)
        {
            IntPtr dataAddress = ReadValue<IntPtr>(addresses.cardGenreBinAddress + 8);

            for (int i = 0; i < Constants.GetNumCards(Version) && i < cardGenres.Length; i++)
            {
                WriteValue<long>(dataAddress + (i * 8), (long)cardGenres[i]);
            }
        }

        /// <summary>
        /// Unlocks all dlc for this play session (doesn't modify any files)
        /// </summary>
        public void UnlockAllDlc()
        {
            ////////////////////////////////////////////////////////////////
            // Unlock chardata_X.bin dlc (challenge duels)
            ////////////////////////////////////////////////////////////////
            IntPtr ptr = addresses.chardataBinAddress;

            // Number of items should be here, if not fall back to 151
            int defaultNumItems = 151;
            int numItems = ReadValue<int>(ReadValue<IntPtr>(ptr - 0x10));
            if (numItems < defaultNumItems || numItems > 1000)
            {
                numItems = defaultNumItems;
            }

            // Note there is an additional blank item at the start of the array
            for (int i = 0; i <= numItems; i++)
            {
                if (i != 0)
                {
                    const int dlcIdOffset = 16;
                    //Debug.WriteLine("chardata dlcId: " + ReadValue<int>(ptr + offset));
                    WriteValue<int>(ptr + dlcIdOffset, 1);
                }
                ptr += 56;
            }

            ////////////////////////////////////////////////////////////////
            // Unlock dueldata_X.bin dlc (campaign duels / decks)
            ////////////////////////////////////////////////////////////////
            ptr = addresses.dueldataBinAddress;

            // Number of items should be here, if not fall back to 140
            defaultNumItems = 140;
            numItems = ReadValue<int>(ReadValue<IntPtr>(ptr - 0x10));
            if (numItems < defaultNumItems || numItems > 1000)
            {
                numItems = defaultNumItems;
            }

            // Note there is an additional blank item at the start of the array
            for (int i = 0; i <= numItems; i++)
            {
                if (i != 0)
                {
                    const int dlcIdOffset = 44;
                    //Debug.WriteLine("dueldata dlcId: " + ReadValue<int>(ptr + offset));
                    WriteValue<int>(ptr + dlcIdOffset, 1);
                }
                ptr += 96;
            }
        }

        /// <summary>
        /// Gets the player index for the given player
        /// </summary>
        public int GetPlayerIndex(Player player)
        {
            return (int)player;
        }
    }
}