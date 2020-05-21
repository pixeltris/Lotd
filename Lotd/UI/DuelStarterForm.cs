using Lotd.FileFormats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lotd.UI
{
    public partial class DuelStarterForm : Form
    {
        private AnimationsForm animationsForm;
        
        private bool shownSpeedMultiplierWarning = false;

        private List<MemTools.YdcDeck> ydcDecks = new List<MemTools.YdcDeck>();
        private List<MemTools.YdcDeck> userDecks = new List<MemTools.YdcDeck>();
        private List<MemTools.YdcDeck> fileDecks = new List<MemTools.YdcDeck>();
        private string deckFilesDir = "Decks";
        private string deckFilesExtension = ".ydl";// this isn't an official file structure
        private string reservedDeckSlotName = "Reserved Slot";

        private MemTools.YdcDeck[] playerDecks = new MemTools.YdcDeck[4];
        private Random rand = new Random();

        private MemTools.YdcDeck nextViewDeck;
        private DateTime lastViewDeck;

        public DuelStarterForm()
        {
            InitializeComponent();

            animationsForm = new AnimationsForm();

            deckTypeComboBox.SelectedIndex = 0;            

            foreach (MemTools.DuelArena arena in Enum.GetValues(typeof(MemTools.DuelArena)))
            {
                duelArenaComboBox.Items.Add(arena);
            }
            duelArenaComboBox.SelectedIndex = 0;

            startingPlayerComboBox.SelectedIndex = 0;
            duelKindComboBox.SelectedIndex = 0;

            foreach (MemTools.ScreenState screen in Enum.GetValues(typeof(MemTools.ScreenState)))
            {
                screenComboBox.Items.Add(screen);
                if (screen == MemTools.ScreenState.MainMenu)
                {
                    screenComboBox.SelectedIndex = screenComboBox.Items.Count - 1;
                }
            }

            if (Program.Version != GameVersion.Lotd)
            {
                // Rewards are handled differently on LE?
                duelRewardsCheckBox.Checked = false;
                duelRewardsCheckBox.Enabled = false;
            }

            ReloadDecks();

            filterDeckXyzCheckBox.CheckStateChanged += FilterDeckCheckBox_CheckStateChanged;
            filterDeckPendulumCheckBox.CheckStateChanged += FilterDeckCheckBox_CheckStateChanged;
            filterDeckRitualCheckBox.CheckStateChanged += FilterDeckCheckBox_CheckStateChanged;
            filterDeckSynchroCheckBox.CheckStateChanged += FilterDeckCheckBox_CheckStateChanged;
            filterDeckFusionCheckBox.CheckStateChanged += FilterDeckCheckBox_CheckStateChanged;

            Program.MemTools.Loaded += MemTools_Loaded;
        }

        private void MemTools_Loaded(object sender, EventArgs e)
        {
            try
            {
                Invoke((MethodInvoker)delegate
               {
                   ReloadDecks();
               });
            }
            catch
            {
            }
        }

        private void FilterDeckCheckBox_CheckStateChanged(object sender, EventArgs e)
        {
            ReloadDecks();
        }

        private void ChildForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            (sender as Form).Hide();
        }

        private void speedMultiplierApplyButton_Click(object sender, EventArgs e)
        {
            if (!Program.MemTools.IsFullyLoaded)
            {
                return;
            }

            if (!shownSpeedMultiplierWarning)
            {
                string warning = "The speed multiplier isn't fully implemented. If you reopen this tool and apply another " +
                    "speed multiplier the game will likely break. Continue anyway?";
                if (MessageBox.Show(warning, "Warning", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
                shownSpeedMultiplierWarning = true;
            }

            Program.MemTools.SetTimeMultiplier((double)speedMultiplierNumericUpDown.Value, true);
        }

        private void goToSceeenButton_Click(object sender, EventArgs e)
        {
            if (!Program.MemTools.IsFullyLoaded)
            {
                return;
            }

            Program.MemTools.SetScreenState((MemTools.ScreenState)screenComboBox.SelectedItem);
        }

        private void unlockContentButtonButton_Click(object sender, EventArgs e)
        {
            SimpleSaveDataForm contentForm = new SimpleSaveDataForm();
            contentForm.ShowDialog();
        }

        private void animationsButton_Click(object sender, EventArgs e)
        {
            animationsForm.Show();
        }

        private void deckFilterTextBox_TextChanged(object sender, EventArgs e)
        {
            UpdateDecksList();
        }

        private void deckTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Program.MemTools.IsFullyLoaded || deckTypeComboBox.SelectedIndex < 0)
            {
                return;
            }

            if (deckTypeComboBox.SelectedIndex == 0)
            {                
                MemTools.YdcDeck[] uDecks = Program.MemTools.ReadUserDecks();
                if (uDecks != null)
                {
                    userDecks.Clear();
                    userDecks.AddRange(uDecks);
                }
            }

            UpdateDecksList();
        }

        private void reloadDecksButton_Click(object sender, EventArgs e)
        {
            ReloadDecks();
        }

        private void ReloadDecks()
        {
            if (!Program.MemTools.IsFullyLoaded)
            {
                return;
            }

            MemTools.YdcDeck[] yDecks = Program.MemTools.ReadYdcDecks();
            if (yDecks != null)
            {
                ydcDecks.Clear();
                
                // The last 4 decks are reserved for custom duels
                for (int i = 0; i < yDecks.Length - 4; i++)
                {
                    ydcDecks.Add(yDecks[i]);
                }
            }
            
            MemTools.YdcDeck[] uDecks = Program.MemTools.ReadUserDecks();
            if (uDecks != null)
            {
                userDecks.Clear();
                userDecks.AddRange(uDecks);
            }
            
            if (Directory.Exists(deckFilesDir))
            {
                fileDecks.Clear();
                foreach (string file in Directory.GetFiles(deckFilesDir, "*" + deckFilesExtension))
                {
                    try
                    {
                        byte[] buffer = File.ReadAllBytes(file);
                        if (buffer.Length != System.Runtime.InteropServices.Marshal.SizeOf(typeof(MemTools.YdcDeck)))
                        {
                            continue;
                        }

                        MemTools.YdcDeck deck = MemTools.StructFromByteArray<MemTools.YdcDeck>(buffer);
                        if (deck.IsValid)
                        {
                            fileDecks.Add(deck);
                        }
                    }
                    catch
                    {
                    }
                }
                foreach (string file in Directory.GetFiles(deckFilesDir, "*" + YdkHelper.FileExtension))
                {
                    try
                    {
                        MemTools.YdcDeck deck = YdkHelper.LoadDeck(file);
                        if (deck.IsValid)
                        {
                            fileDecks.Add(deck);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            UpdateDecksList();
        }

        private void exportDeckButton_Click(object sender, EventArgs e)
        {
            MemTools.YdcDeck deck = GetSelectedDeck();
            if (deck.Equals(default(MemTools.YdcDeck)))
            {
                return;
            }

            try
            {
                if (!Directory.Exists(deckFilesDir))
                {
                    Directory.CreateDirectory(deckFilesDir);
                }
            }
            catch
            {
            }

            if (Directory.Exists(deckFilesDir))
            {
                string filename = MakeSafeFileName(deck.DeckName).Trim();
                if (string.IsNullOrEmpty(filename))
                {
                    return;
                }
                filename += deckFilesExtension;
                string path = Path.Combine(deckFilesDir, filename);
                try
                {
                    DialogResult result = MessageBox.Show("Would you like to export this as YDK (instead of YDL)?", "Export", MessageBoxButtons.YesNoCancel);
                    if (result == DialogResult.Yes)
                    {
                        YdkHelper.SaveDeck(deck, Path.ChangeExtension(path, YdkHelper.FileExtension));
                    }
                    else if (result == DialogResult.No)
                    {
                        File.WriteAllBytes(path, MemTools.StructToByteArray(deck));
                    }
                }
                catch
                {
                }
            }
        }

        private string MakeSafeFileName(string filename)
        {
            string replaceChar = "_";
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c.ToString(), replaceChar);
            }
            return filename;
        }

        private void viewDeckButton_Click(object sender, EventArgs e)
        {
            ViewDeck(GetSelectedDeck());
        }

        private void setPlayer1DeckButton_Click(object sender, EventArgs e)
        {
            SetPlayerDeck(GetSelectedDeck(), 0, false);
        }

        private void setPlayer2DeckButton_Click(object sender, EventArgs e)
        {
            SetPlayerDeck(GetSelectedDeck(), 1, false);
        }

        private void setPlayer3Button_Click(object sender, EventArgs e)
        {
            SetPlayerDeck(GetSelectedDeck(), 2, false);
        }

        private void setPlayer4Button_Click(object sender, EventArgs e)
        {
            SetPlayerDeck(GetSelectedDeck(), 3, false);
        }

        private void clearPlayerDecksButton_Click(object sender, EventArgs e)
        {
            SetPlayerDeck(default(MemTools.YdcDeck), 0, true);
            SetPlayerDeck(default(MemTools.YdcDeck), 1, true);
            SetPlayerDeck(default(MemTools.YdcDeck), 2, true);
            SetPlayerDeck(default(MemTools.YdcDeck), 3, true);
        }

        private void SetPlayerDeck(MemTools.YdcDeck deck, int playerIndex, bool allowNull)
        {
            if (!allowNull && deck.Equals(default(MemTools.YdcDeck)))
            {
                return;
            }

            playerDecks[playerIndex] = deck;
            switch (playerIndex)
            {
                case 0:
                    player1DeckLabel.Text = "P1: " + deck.DeckName;
                    break;
                case 1:
                    player2DeckLabel.Text = "P2: " + deck.DeckName;
                    break;
                case 2:
                    player3DeckLabel.Text = "P3: " + deck.DeckName;
                    break;
                case 3:
                    player4DeckLabel.Text = "P4: " + deck.DeckName;
                    break;
            }
        }

        private void decksListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            MemTools.YdcDeck deck = GetSelectedDeck();
            if (!deck.Equals(default(MemTools.YdcDeck)))
            {
                if (autoViewDeckCheckBox.Checked)
                {
                    ViewDeck(deck);
                }
            }
        }

        private void UpdateDecksList()
        {
            ydcDecks.Sort((x, y) => (x.DeckName != null ? x.DeckName : string.Empty).CompareTo(
                y.DeckName != null ? y.DeckName : string.Empty));
            userDecks.Sort((x, y) => (x.DeckName != null ? x.DeckName : string.Empty).CompareTo(
                y.DeckName != null ? y.DeckName : string.Empty));
            fileDecks.Sort((x, y) => (x.DeckName != null ? x.DeckName : string.Empty).CompareTo(
                y.DeckName != null ? y.DeckName : string.Empty));

            decksListBox.BeginUpdate();
            decksListBox.Items.Clear();

            string filter = deckFilterTextBox.Text;
            if (string.IsNullOrEmpty(filter))
            {
                filter = string.Empty;
            }
            filter = filter.ToLower().Trim();

            bool isUserDecks = false;
            List<MemTools.YdcDeck> decks = null;
            switch (deckTypeComboBox.SelectedIndex)
            {
                case 0:
                    decks = userDecks;
                    isUserDecks = true;
                    break;
                case 1:
                    decks = ydcDecks;
                    break;
                case 2:
                    decks = fileDecks;
                    break;
            }
            if (decks != null)
            {
                for(int i = 0; i < decks.Count; i++)
                {
                    MemTools.YdcDeck deck = decks[i];

                    HashSet<short> cardIds = new HashSet<short>();
                    for (int j = 0; j < deck.NumMainDeckCards && j < deck.MainDeck.Length; j++)
                    {
                        cardIds.Add(deck.MainDeck[j]);
                    }
                    for (int j = 0; j < deck.NumExtraDeckCards && j < deck.ExtraDeck.Length; j++)
                    {
                        cardIds.Add(deck.ExtraDeck[j]);
                    }
                    for (int j = 0; j < deck.NumSideDeckCards && j < deck.SideDeck.Length; j++)
                    {
                        cardIds.Add(deck.SideDeck[j]);
                    }

                    bool hasXyz = false;
                    bool hasPendulum = false;
                    bool hasRitual = false;
                    bool hasSynchro = false;
                    bool hasFusion = false;
                    bool hasLink = false;
                    foreach (short cardId in cardIds)
                    {
                        CardInfo card;
                        if (Program.Manager.CardManager.Cards.TryGetValue(cardId, out card))
                        {
                            if (card.CardTypeFlags.HasFlag(CardTypeFlags.Xyz))
                            {
                                hasXyz = true;
                            }
                            if (card.CardTypeFlags.HasFlag(CardTypeFlags.Pendulum))
                            {
                                hasPendulum = true;
                            }
                            if (card.CardTypeFlags.HasFlag(CardTypeFlags.Ritual))
                            {
                                hasRitual = true;
                            }
                            if (card.CardTypeFlags.HasFlag(CardTypeFlags.Synchro) ||
                                card.CardTypeFlags.HasFlag(CardTypeFlags.DarkSynchro))
                            {
                                hasSynchro = true;
                            }
                            if (card.CardTypeFlags.HasFlag(CardTypeFlags.Fusion))
                            {
                                hasFusion = true;
                            }
                            if (card.CardTypeFlags.HasFlag(CardTypeFlags.Link))
                            {
                                hasLink = true;
                            }
                        }
                    }
                    if (!FilterMatched(filterDeckXyzCheckBox, hasXyz) ||
                        !FilterMatched(filterDeckPendulumCheckBox, hasPendulum) ||
                        !FilterMatched(filterDeckRitualCheckBox, hasRitual) ||
                        !FilterMatched(filterDeckSynchroCheckBox, hasSynchro) ||
                        !FilterMatched(filterDeckFusionCheckBox, hasFusion) ||
                        !FilterMatched(filterDeckLinkCheckBox, hasLink))
                    {
                        continue;
                    }

                    if (deck.IsDeckComplete || (isUserDecks && !string.IsNullOrEmpty(deck.DeckName)))
                    {
                        if (isUserDecks && deck.DeckName == reservedDeckSlotName && i == decks.Count - 1)
                        {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(filter) &&
                            !(deck.DeckName != null ? deck.DeckName : string.Empty).ToLower().Contains(filter))
                        {
                            continue;
                        }

                        decksListBox.Items.Add(new YdcDeckWrapper(deck));
                    }
                }
            }
            decksListBox.EndUpdate();
        }

        private bool FilterMatched(CheckBox checkbox, bool state)
        {
            switch (checkbox.CheckState)
            {
                default:
                case CheckState.Unchecked: return !state;
                case CheckState.Indeterminate: return state;
                case CheckState.Checked: return true;
            }
        }

        private MemTools.YdcDeck GetSelectedDeck()
        {
            if (deckTypeComboBox.SelectedIndex >= 0 &&
                decksListBox.SelectedIndex >= 0)
            {
                return ((YdcDeckWrapper)decksListBox.SelectedItem).Deck;
            }

            return default(MemTools.YdcDeck);
        }

        private void startDuelButton_Click(object sender, EventArgs e)
        {
            if (!Program.MemTools.IsFullyLoaded)
            {
                return;
            }

            // If we let a duel start at the game logo screen the user game data will be wiped once the duel
            // finishes as user game data is loaded when pressing any key at the game logo screen
            if (Program.MemTools.GetScreenState() == MemTools.ScreenState.GameLogo)
            {
                return;
            }

            MemTools.StartDuelInfo startDuelInfo = new MemTools.StartDuelInfo();
            startDuelInfo.SetController(MemTools.Player.Self,
                player1AICheckBox.Checked ? MemTools.PlayerController.AI : MemTools.PlayerController.Player);
            startDuelInfo.SetController(MemTools.Player.Opponent,
                player2AICheckBox.Checked ? MemTools.PlayerController.AI : MemTools.PlayerController.Player);
            startDuelInfo.SetController(MemTools.Player.TagSelf,
                player3AICheckBox.Checked ? MemTools.PlayerController.AI : MemTools.PlayerController.Player);
            startDuelInfo.SetController(MemTools.Player.TagOpponent,
                player4AICheckBox.Checked ? MemTools.PlayerController.AI : MemTools.PlayerController.Player);            
            startDuelInfo.UseSpeedDuelLifePoints = false;
            startDuelInfo.SpeedDuel = duelKindComboBox.SelectedIndex == 1;
            startDuelInfo.RushDuel = duelKindComboBox.SelectedIndex == 2;
            startDuelInfo.TagDuel = tagDuelCheckBox.Checked;
            startDuelInfo.Match = matchDuelCheckBox.Checked;
            startDuelInfo.Arena = (MemTools.DuelArena)duelArenaComboBox.SelectedItem;
            switch (startingPlayerComboBox.SelectedIndex)
            {
                case 0:
                    startDuelInfo.StartingPlayer = rand.Next(2) == 0 ? MemTools.Player.Self : MemTools.Player.Opponent;
                    break;
                case 1:
                    startDuelInfo.StartingPlayer = MemTools.Player.Self;
                    break;
                case 2:
                    startDuelInfo.StartingPlayer = MemTools.Player.Opponent;
                    break;
            }
            if (startDuelInfo.GetController(MemTools.Player.Self) == MemTools.PlayerController.AI &&
                startDuelInfo.GetController(MemTools.Player.Opponent) == MemTools.PlayerController.AI &&
                !skipRockPaperScissorsCheckBox.Checked)
            {
                startDuelInfo.AIMode = MemTools.AIMode.AIVsAI;
            }
            startDuelInfo.SkipRockPaperScissors = skipRockPaperScissorsCheckBox.Checked;
            startDuelInfo.LifePoints = (int)lifePointsNumericUpDown.Value;
            startDuelInfo.StartingHandCount = (int)startingHandNumericUpDown.Value;
            startDuelInfo.RandSeed = (int)(uint)seedNumericUpDown.Value;
            startDuelInfo.FullReload = fullReloadCheckBox.Checked;

            startDuelInfo.SetAvatarId(MemTools.Player.Self, playerDecks[0].DeckAvatarId);
            startDuelInfo.SetAvatarId(MemTools.Player.Opponent, playerDecks[1].DeckAvatarId);
            startDuelInfo.SetAvatarId(MemTools.Player.TagSelf, playerDecks[2].DeckAvatarId);
            startDuelInfo.SetAvatarId(MemTools.Player.TagOpponent, playerDecks[3].DeckAvatarId);

            // Wipe over the last ydc deck slots as they are unused anyway
            MemTools.YdcDeck[] ydcDecks = Program.MemTools.ReadYdcDecks();
            if (ydcDecks == null)
            {
                return;
            }
            int p1Index = ydcDecks.Length - 1;
            int p2Index = ydcDecks.Length - 2;
            int p3Index = ydcDecks.Length - 3;
            int p4Index = ydcDecks.Length - 4;
            ydcDecks[p1Index] = playerDecks[0];
            ydcDecks[p2Index] = playerDecks[1];
            ydcDecks[p3Index] = playerDecks[2];
            ydcDecks[p4Index] = playerDecks[3];
            Program.MemTools.WriteYdcDecks(ydcDecks);

            startDuelInfo.SetDeckId(MemTools.Player.Self, Constants.DeckIndexYdcStart + p1Index);
            startDuelInfo.SetDeckId(MemTools.Player.Opponent, Constants.DeckIndexYdcStart + p2Index);
            startDuelInfo.SetDeckId(MemTools.Player.TagSelf, Constants.DeckIndexYdcStart + p3Index);
            startDuelInfo.SetDeckId(MemTools.Player.TagOpponent, Constants.DeckIndexYdcStart + p4Index);

            if (duelRewardsCheckBox.Checked)
            {
                // Set the type of duel to challenge duel so that rewards are obtained (when the user
                // finishes the duel they will end up in the challenge duel screen)
                startDuelInfo.DuelType = MemTools.DuelType.Challenge;
            }

            Program.MemTools.StartDuel(startDuelInfo);
        }

        private void ViewDeck(MemTools.YdcDeck deck)
        {
            if (deck.Equals(default(MemTools.YdcDeck)))
            {
                return;
            }

            if (lastViewDeck < DateTime.Now - TimeSpan.FromSeconds(0.5))
            {
                ViewDeckNow(deck);
            }
            else
            {
                nextViewDeck = deck;
                viewDeckDelayTimer.Enabled = true;
            }
        }

        private void viewDeckDelayTimer_Tick(object sender, EventArgs e)
        {
            // Throttle viewing decks as there is a crash if loading too fast
            if (!nextViewDeck.Equals(default(MemTools.YdcDeck)))
            {
                ViewDeckNow(nextViewDeck);
                nextViewDeck = default(MemTools.YdcDeck);
                viewDeckDelayTimer.Enabled = false;
            }
        }

        private void ViewDeckNow(MemTools.YdcDeck deck)
        {
            if (!Program.MemTools.IsFullyLoaded)
            {
                return;
            }

            if (deck.Equals(default(MemTools.YdcDeck)))
            {
                return;
            }

            MemTools.ScreenState screenState = Program.MemTools.GetScreenState();
            if (screenState == MemTools.ScreenState.Duel ||
                screenState == MemTools.ScreenState.DuelLoadingScreen)
            {
                // Don't interrupt a duel
                return;
            }

            lastViewDeck = DateTime.Now;
            MemTools.YdcDeck[] userDecks = Program.MemTools.ReadUserDecks();
            if (userDecks != null)
            {
                string deckName = reservedDeckSlotName;
                int lastDeckIndex = userDecks.Length - 1;
                MemTools.YdcDeck lastDeck = userDecks[lastDeckIndex];

                if (lastDeck.DeckName != deckName)
                {
                    if (MessageBox.Show("This will wipe over the deck in user slot 32. Continue?", "Warning", MessageBoxButtons.YesNo) !=
                        DialogResult.Yes)
                    {
                        return;
                    }
                }

                lastDeck = deck;
                lastDeck.DeckName = deckName;
                userDecks[lastDeckIndex] = lastDeck;

                Program.MemTools.WriteUserDecks(userDecks);
                Program.MemTools.SelectUserDeck(lastDeckIndex, true);
            }
        }

        private void useCustomHandCountLP_CheckedChanged(object sender, EventArgs e)
        {
            lifePointsNumericUpDown.Enabled = useCustomHandCountLP.Checked;
            startingHandNumericUpDown.Enabled = useCustomHandCountLP.Checked;
            UpdateAutoLifePointsHandCount();
        }

        private void duelKindComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateAutoLifePointsHandCount();
        }

        private void UpdateAutoLifePointsHandCount()
        {
            if (!useCustomHandCountLP.Checked)
            {
                switch (duelKindComboBox.SelectedIndex)
                {
                    case 0:
                        lifePointsNumericUpDown.Value = 8000;
                        startingHandNumericUpDown.Value = 5;
                        break;
                    case 1:
                        lifePointsNumericUpDown.Value = 4000;
                        startingHandNumericUpDown.Value = 5;
                        break;
                    case 2:
                        lifePointsNumericUpDown.Value = 8000;
                        startingHandNumericUpDown.Value = 4;
                        break;
                }
            }
        }

        private class YdcDeckWrapper
        {
            public MemTools.YdcDeck Deck;

            public YdcDeckWrapper(MemTools.YdcDeck deck)
            {
                Deck = deck;
            }

            public override string ToString()
            {
                return Deck.DeckName;
            }
        }
    }
}
