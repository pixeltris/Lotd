namespace Lotd.UI
{
    partial class DuelStarterForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tagDuelCheckBox = new System.Windows.Forms.CheckBox();
            this.matchDuelCheckBox = new System.Windows.Forms.CheckBox();
            this.decksListBox = new System.Windows.Forms.ListBox();
            this.decksListPanel1 = new System.Windows.Forms.Panel();
            this.deckFilterTextBox = new System.Windows.Forms.TextBox();
            this.playerDeckButtonsPanel = new System.Windows.Forms.Panel();
            this.clearPlayerDecksButton = new System.Windows.Forms.Button();
            this.setPlayer4Button = new System.Windows.Forms.Button();
            this.setPlayer3Button = new System.Windows.Forms.Button();
            this.setPlayer2DeckButton = new System.Windows.Forms.Button();
            this.setPlayer1DeckButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.autoViewDeckCheckBox = new System.Windows.Forms.CheckBox();
            this.viewDeckButton = new System.Windows.Forms.Button();
            this.exportDeckButton = new System.Windows.Forms.Button();
            this.reloadDecksButton = new System.Windows.Forms.Button();
            this.deckTypeComboBox = new System.Windows.Forms.ComboBox();
            this.deckFilterPanel = new System.Windows.Forms.Panel();
            this.filterDeckRitualCheckBox = new System.Windows.Forms.CheckBox();
            this.filterDeckXyzCheckBox = new System.Windows.Forms.CheckBox();
            this.filterDeckFusionCheckBox = new System.Windows.Forms.CheckBox();
            this.filterDeckSynchroCheckBox = new System.Windows.Forms.CheckBox();
            this.filterDeckPendulumCheckBox = new System.Windows.Forms.CheckBox();
            this.player1DeckLabel = new System.Windows.Forms.Label();
            this.duelArenaComboBox = new System.Windows.Forms.ComboBox();
            this.speedDuelCheckBox = new System.Windows.Forms.CheckBox();
            this.lifePointsLabel = new System.Windows.Forms.Label();
            this.lifePointsNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.skipRockPaperScissorsCheckBox = new System.Windows.Forms.CheckBox();
            this.player2DeckLabel = new System.Windows.Forms.Label();
            this.player4DeckLabel = new System.Windows.Forms.Label();
            this.player3DeckLabel = new System.Windows.Forms.Label();
            this.startingHandNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.startHandLabel = new System.Windows.Forms.Label();
            this.startDuelButton = new System.Windows.Forms.Button();
            this.player1AICheckBox = new System.Windows.Forms.CheckBox();
            this.player2AICheckBox = new System.Windows.Forms.CheckBox();
            this.player3AICheckBox = new System.Windows.Forms.CheckBox();
            this.player4AICheckBox = new System.Windows.Forms.CheckBox();
            this.viewDeckDelayTimer = new System.Windows.Forms.Timer(this.components);
            this.startingPlayerComboBox = new System.Windows.Forms.ComboBox();
            this.fullReloadCheckBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.speedMultiplierNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.speedMultiplierLabel = new System.Windows.Forms.Label();
            this.speedMultiplierApplyButton = new System.Windows.Forms.Button();
            this.animationsButton = new System.Windows.Forms.Button();
            this.unlockContentButtonButton = new System.Windows.Forms.Button();
            this.duelSettingsPanel = new System.Windows.Forms.Panel();
            this.duelRewardsCheckBox = new System.Windows.Forms.CheckBox();
            this.setsDecksPanel = new System.Windows.Forms.Panel();
            this.decksListPanel1.SuspendLayout();
            this.playerDeckButtonsPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.deckFilterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lifePointsNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.startingHandNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.speedMultiplierNumericUpDown)).BeginInit();
            this.duelSettingsPanel.SuspendLayout();
            this.setsDecksPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tagDuelCheckBox
            // 
            this.tagDuelCheckBox.AutoSize = true;
            this.tagDuelCheckBox.Location = new System.Drawing.Point(5, 28);
            this.tagDuelCheckBox.Name = "tagDuelCheckBox";
            this.tagDuelCheckBox.Size = new System.Drawing.Size(70, 17);
            this.tagDuelCheckBox.TabIndex = 3;
            this.tagDuelCheckBox.Text = "Tag Duel";
            this.tagDuelCheckBox.UseVisualStyleBackColor = true;
            // 
            // matchDuelCheckBox
            // 
            this.matchDuelCheckBox.AutoSize = true;
            this.matchDuelCheckBox.Location = new System.Drawing.Point(5, 51);
            this.matchDuelCheckBox.Name = "matchDuelCheckBox";
            this.matchDuelCheckBox.Size = new System.Drawing.Size(81, 17);
            this.matchDuelCheckBox.TabIndex = 4;
            this.matchDuelCheckBox.Text = "Match Duel";
            this.matchDuelCheckBox.UseVisualStyleBackColor = true;
            // 
            // decksListBox
            // 
            this.decksListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.decksListBox.FormattingEnabled = true;
            this.decksListBox.IntegralHeight = false;
            this.decksListBox.Location = new System.Drawing.Point(0, 136);
            this.decksListBox.Name = "decksListBox";
            this.decksListBox.Size = new System.Drawing.Size(217, 314);
            this.decksListBox.TabIndex = 5;
            this.decksListBox.SelectedIndexChanged += new System.EventHandler(this.decksListBox_SelectedIndexChanged);
            // 
            // decksListPanel1
            // 
            this.decksListPanel1.Controls.Add(this.decksListBox);
            this.decksListPanel1.Controls.Add(this.deckFilterTextBox);
            this.decksListPanel1.Controls.Add(this.playerDeckButtonsPanel);
            this.decksListPanel1.Controls.Add(this.panel1);
            this.decksListPanel1.Controls.Add(this.deckTypeComboBox);
            this.decksListPanel1.Controls.Add(this.deckFilterPanel);
            this.decksListPanel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.decksListPanel1.Location = new System.Drawing.Point(0, 0);
            this.decksListPanel1.Name = "decksListPanel1";
            this.decksListPanel1.Size = new System.Drawing.Size(217, 450);
            this.decksListPanel1.TabIndex = 6;
            // 
            // deckFilterTextBox
            // 
            this.deckFilterTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.deckFilterTextBox.Location = new System.Drawing.Point(0, 116);
            this.deckFilterTextBox.Name = "deckFilterTextBox";
            this.deckFilterTextBox.Size = new System.Drawing.Size(217, 20);
            this.deckFilterTextBox.TabIndex = 17;
            this.deckFilterTextBox.TextChanged += new System.EventHandler(this.deckFilterTextBox_TextChanged);
            // 
            // playerDeckButtonsPanel
            // 
            this.playerDeckButtonsPanel.Controls.Add(this.clearPlayerDecksButton);
            this.playerDeckButtonsPanel.Controls.Add(this.setPlayer4Button);
            this.playerDeckButtonsPanel.Controls.Add(this.setPlayer3Button);
            this.playerDeckButtonsPanel.Controls.Add(this.setPlayer2DeckButton);
            this.playerDeckButtonsPanel.Controls.Add(this.setPlayer1DeckButton);
            this.playerDeckButtonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.playerDeckButtonsPanel.Location = new System.Drawing.Point(0, 93);
            this.playerDeckButtonsPanel.Name = "playerDeckButtonsPanel";
            this.playerDeckButtonsPanel.Size = new System.Drawing.Size(217, 23);
            this.playerDeckButtonsPanel.TabIndex = 10;
            // 
            // clearPlayerDecksButton
            // 
            this.clearPlayerDecksButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.clearPlayerDecksButton.Location = new System.Drawing.Point(160, 0);
            this.clearPlayerDecksButton.Name = "clearPlayerDecksButton";
            this.clearPlayerDecksButton.Size = new System.Drawing.Size(45, 23);
            this.clearPlayerDecksButton.TabIndex = 20;
            this.clearPlayerDecksButton.Text = "Clear";
            this.clearPlayerDecksButton.UseVisualStyleBackColor = true;
            this.clearPlayerDecksButton.Click += new System.EventHandler(this.clearPlayerDecksButton_Click);
            // 
            // setPlayer4Button
            // 
            this.setPlayer4Button.Dock = System.Windows.Forms.DockStyle.Left;
            this.setPlayer4Button.Location = new System.Drawing.Point(120, 0);
            this.setPlayer4Button.Name = "setPlayer4Button";
            this.setPlayer4Button.Size = new System.Drawing.Size(40, 23);
            this.setPlayer4Button.TabIndex = 19;
            this.setPlayer4Button.Text = "P4";
            this.setPlayer4Button.UseVisualStyleBackColor = true;
            this.setPlayer4Button.Click += new System.EventHandler(this.setPlayer4Button_Click);
            // 
            // setPlayer3Button
            // 
            this.setPlayer3Button.Dock = System.Windows.Forms.DockStyle.Left;
            this.setPlayer3Button.Location = new System.Drawing.Point(80, 0);
            this.setPlayer3Button.Name = "setPlayer3Button";
            this.setPlayer3Button.Size = new System.Drawing.Size(40, 23);
            this.setPlayer3Button.TabIndex = 18;
            this.setPlayer3Button.Text = "P3";
            this.setPlayer3Button.UseVisualStyleBackColor = true;
            this.setPlayer3Button.Click += new System.EventHandler(this.setPlayer3Button_Click);
            // 
            // setPlayer2DeckButton
            // 
            this.setPlayer2DeckButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.setPlayer2DeckButton.Location = new System.Drawing.Point(40, 0);
            this.setPlayer2DeckButton.Name = "setPlayer2DeckButton";
            this.setPlayer2DeckButton.Size = new System.Drawing.Size(40, 23);
            this.setPlayer2DeckButton.TabIndex = 17;
            this.setPlayer2DeckButton.Text = "P2";
            this.setPlayer2DeckButton.UseVisualStyleBackColor = true;
            this.setPlayer2DeckButton.Click += new System.EventHandler(this.setPlayer2DeckButton_Click);
            // 
            // setPlayer1DeckButton
            // 
            this.setPlayer1DeckButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.setPlayer1DeckButton.Location = new System.Drawing.Point(0, 0);
            this.setPlayer1DeckButton.Name = "setPlayer1DeckButton";
            this.setPlayer1DeckButton.Size = new System.Drawing.Size(40, 23);
            this.setPlayer1DeckButton.TabIndex = 16;
            this.setPlayer1DeckButton.Text = "P1";
            this.setPlayer1DeckButton.UseVisualStyleBackColor = true;
            this.setPlayer1DeckButton.Click += new System.EventHandler(this.setPlayer1DeckButton_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.autoViewDeckCheckBox);
            this.panel1.Controls.Add(this.viewDeckButton);
            this.panel1.Controls.Add(this.exportDeckButton);
            this.panel1.Controls.Add(this.reloadDecksButton);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 70);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(217, 23);
            this.panel1.TabIndex = 16;
            // 
            // autoViewDeckCheckBox
            // 
            this.autoViewDeckCheckBox.AutoSize = true;
            this.autoViewDeckCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.autoViewDeckCheckBox.Location = new System.Drawing.Point(145, 0);
            this.autoViewDeckCheckBox.Name = "autoViewDeckCheckBox";
            this.autoViewDeckCheckBox.Size = new System.Drawing.Size(74, 23);
            this.autoViewDeckCheckBox.TabIndex = 9;
            this.autoViewDeckCheckBox.Text = "Auto View";
            this.autoViewDeckCheckBox.UseVisualStyleBackColor = true;
            // 
            // viewDeckButton
            // 
            this.viewDeckButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.viewDeckButton.Location = new System.Drawing.Point(101, 0);
            this.viewDeckButton.Name = "viewDeckButton";
            this.viewDeckButton.Size = new System.Drawing.Size(44, 23);
            this.viewDeckButton.TabIndex = 7;
            this.viewDeckButton.Text = "View";
            this.viewDeckButton.UseVisualStyleBackColor = true;
            this.viewDeckButton.Click += new System.EventHandler(this.viewDeckButton_Click);
            // 
            // exportDeckButton
            // 
            this.exportDeckButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.exportDeckButton.Location = new System.Drawing.Point(53, 0);
            this.exportDeckButton.Name = "exportDeckButton";
            this.exportDeckButton.Size = new System.Drawing.Size(48, 23);
            this.exportDeckButton.TabIndex = 11;
            this.exportDeckButton.Text = "Export";
            this.exportDeckButton.UseVisualStyleBackColor = true;
            this.exportDeckButton.Click += new System.EventHandler(this.exportDeckButton_Click);
            // 
            // reloadDecksButton
            // 
            this.reloadDecksButton.Dock = System.Windows.Forms.DockStyle.Left;
            this.reloadDecksButton.Location = new System.Drawing.Point(0, 0);
            this.reloadDecksButton.Name = "reloadDecksButton";
            this.reloadDecksButton.Size = new System.Drawing.Size(53, 23);
            this.reloadDecksButton.TabIndex = 12;
            this.reloadDecksButton.Text = "Reload";
            this.reloadDecksButton.UseVisualStyleBackColor = true;
            this.reloadDecksButton.Click += new System.EventHandler(this.reloadDecksButton_Click);
            // 
            // deckTypeComboBox
            // 
            this.deckTypeComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.deckTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.deckTypeComboBox.FormattingEnabled = true;
            this.deckTypeComboBox.Items.AddRange(new object[] {
            "User",
            "Game",
            "File"});
            this.deckTypeComboBox.Location = new System.Drawing.Point(0, 49);
            this.deckTypeComboBox.Name = "deckTypeComboBox";
            this.deckTypeComboBox.Size = new System.Drawing.Size(217, 21);
            this.deckTypeComboBox.TabIndex = 8;
            this.deckTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.deckTypeComboBox_SelectedIndexChanged);
            // 
            // deckFilterPanel
            // 
            this.deckFilterPanel.Controls.Add(this.filterDeckRitualCheckBox);
            this.deckFilterPanel.Controls.Add(this.filterDeckXyzCheckBox);
            this.deckFilterPanel.Controls.Add(this.filterDeckFusionCheckBox);
            this.deckFilterPanel.Controls.Add(this.filterDeckSynchroCheckBox);
            this.deckFilterPanel.Controls.Add(this.filterDeckPendulumCheckBox);
            this.deckFilterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.deckFilterPanel.Location = new System.Drawing.Point(0, 0);
            this.deckFilterPanel.Name = "deckFilterPanel";
            this.deckFilterPanel.Size = new System.Drawing.Size(217, 49);
            this.deckFilterPanel.TabIndex = 18;
            // 
            // filterDeckRitualCheckBox
            // 
            this.filterDeckRitualCheckBox.AutoSize = true;
            this.filterDeckRitualCheckBox.Checked = true;
            this.filterDeckRitualCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.filterDeckRitualCheckBox.Location = new System.Drawing.Point(69, 28);
            this.filterDeckRitualCheckBox.Name = "filterDeckRitualCheckBox";
            this.filterDeckRitualCheckBox.Size = new System.Drawing.Size(53, 17);
            this.filterDeckRitualCheckBox.TabIndex = 38;
            this.filterDeckRitualCheckBox.Text = "Ritual";
            this.filterDeckRitualCheckBox.ThreeState = true;
            this.filterDeckRitualCheckBox.UseVisualStyleBackColor = true;
            // 
            // filterDeckXyzCheckBox
            // 
            this.filterDeckXyzCheckBox.AutoSize = true;
            this.filterDeckXyzCheckBox.Checked = true;
            this.filterDeckXyzCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.filterDeckXyzCheckBox.Location = new System.Drawing.Point(5, 5);
            this.filterDeckXyzCheckBox.Name = "filterDeckXyzCheckBox";
            this.filterDeckXyzCheckBox.Size = new System.Drawing.Size(43, 17);
            this.filterDeckXyzCheckBox.TabIndex = 34;
            this.filterDeckXyzCheckBox.Text = "Xyz";
            this.filterDeckXyzCheckBox.ThreeState = true;
            this.filterDeckXyzCheckBox.UseVisualStyleBackColor = true;
            // 
            // filterDeckFusionCheckBox
            // 
            this.filterDeckFusionCheckBox.AutoSize = true;
            this.filterDeckFusionCheckBox.Checked = true;
            this.filterDeckFusionCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.filterDeckFusionCheckBox.Location = new System.Drawing.Point(5, 28);
            this.filterDeckFusionCheckBox.Name = "filterDeckFusionCheckBox";
            this.filterDeckFusionCheckBox.Size = new System.Drawing.Size(57, 17);
            this.filterDeckFusionCheckBox.TabIndex = 37;
            this.filterDeckFusionCheckBox.Text = "Fusion";
            this.filterDeckFusionCheckBox.ThreeState = true;
            this.filterDeckFusionCheckBox.UseVisualStyleBackColor = true;
            // 
            // filterDeckSynchroCheckBox
            // 
            this.filterDeckSynchroCheckBox.AutoSize = true;
            this.filterDeckSynchroCheckBox.Checked = true;
            this.filterDeckSynchroCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.filterDeckSynchroCheckBox.Location = new System.Drawing.Point(54, 5);
            this.filterDeckSynchroCheckBox.Name = "filterDeckSynchroCheckBox";
            this.filterDeckSynchroCheckBox.Size = new System.Drawing.Size(65, 17);
            this.filterDeckSynchroCheckBox.TabIndex = 35;
            this.filterDeckSynchroCheckBox.Text = "Synchro";
            this.filterDeckSynchroCheckBox.ThreeState = true;
            this.filterDeckSynchroCheckBox.UseVisualStyleBackColor = true;
            // 
            // filterDeckPendulumCheckBox
            // 
            this.filterDeckPendulumCheckBox.AutoSize = true;
            this.filterDeckPendulumCheckBox.Checked = true;
            this.filterDeckPendulumCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.filterDeckPendulumCheckBox.Location = new System.Drawing.Point(125, 5);
            this.filterDeckPendulumCheckBox.Name = "filterDeckPendulumCheckBox";
            this.filterDeckPendulumCheckBox.Size = new System.Drawing.Size(73, 17);
            this.filterDeckPendulumCheckBox.TabIndex = 36;
            this.filterDeckPendulumCheckBox.Text = "Pendulum";
            this.filterDeckPendulumCheckBox.ThreeState = true;
            this.filterDeckPendulumCheckBox.UseVisualStyleBackColor = true;
            // 
            // player1DeckLabel
            // 
            this.player1DeckLabel.AutoSize = true;
            this.player1DeckLabel.Location = new System.Drawing.Point(3, 265);
            this.player1DeckLabel.Name = "player1DeckLabel";
            this.player1DeckLabel.Size = new System.Drawing.Size(26, 13);
            this.player1DeckLabel.TabIndex = 7;
            this.player1DeckLabel.Text = "P1: ";
            // 
            // duelArenaComboBox
            // 
            this.duelArenaComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.duelArenaComboBox.FormattingEnabled = true;
            this.duelArenaComboBox.Location = new System.Drawing.Point(80, 189);
            this.duelArenaComboBox.Name = "duelArenaComboBox";
            this.duelArenaComboBox.Size = new System.Drawing.Size(146, 21);
            this.duelArenaComboBox.TabIndex = 10;
            // 
            // speedDuelCheckBox
            // 
            this.speedDuelCheckBox.AutoSize = true;
            this.speedDuelCheckBox.Location = new System.Drawing.Point(5, 74);
            this.speedDuelCheckBox.Name = "speedDuelCheckBox";
            this.speedDuelCheckBox.Size = new System.Drawing.Size(82, 17);
            this.speedDuelCheckBox.TabIndex = 11;
            this.speedDuelCheckBox.Text = "Speed Duel";
            this.speedDuelCheckBox.UseVisualStyleBackColor = true;
            // 
            // lifePointsLabel
            // 
            this.lifePointsLabel.AutoSize = true;
            this.lifePointsLabel.Location = new System.Drawing.Point(4, 220);
            this.lifePointsLabel.Name = "lifePointsLabel";
            this.lifePointsLabel.Size = new System.Drawing.Size(59, 13);
            this.lifePointsLabel.TabIndex = 13;
            this.lifePointsLabel.Text = "Life Points:";
            // 
            // lifePointsNumericUpDown
            // 
            this.lifePointsNumericUpDown.Location = new System.Drawing.Point(67, 216);
            this.lifePointsNumericUpDown.Maximum = new decimal(new int[] {
            60000,
            0,
            0,
            0});
            this.lifePointsNumericUpDown.Name = "lifePointsNumericUpDown";
            this.lifePointsNumericUpDown.Size = new System.Drawing.Size(120, 20);
            this.lifePointsNumericUpDown.TabIndex = 14;
            this.lifePointsNumericUpDown.Value = new decimal(new int[] {
            8000,
            0,
            0,
            0});
            // 
            // skipRockPaperScissorsCheckBox
            // 
            this.skipRockPaperScissorsCheckBox.AutoSize = true;
            this.skipRockPaperScissorsCheckBox.Checked = true;
            this.skipRockPaperScissorsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.skipRockPaperScissorsCheckBox.Location = new System.Drawing.Point(5, 97);
            this.skipRockPaperScissorsCheckBox.Name = "skipRockPaperScissorsCheckBox";
            this.skipRockPaperScissorsCheckBox.Size = new System.Drawing.Size(149, 17);
            this.skipRockPaperScissorsCheckBox.TabIndex = 15;
            this.skipRockPaperScissorsCheckBox.Text = "Skip Rock Paper Scissors";
            this.skipRockPaperScissorsCheckBox.UseVisualStyleBackColor = true;
            // 
            // player2DeckLabel
            // 
            this.player2DeckLabel.AutoSize = true;
            this.player2DeckLabel.Location = new System.Drawing.Point(3, 284);
            this.player2DeckLabel.Name = "player2DeckLabel";
            this.player2DeckLabel.Size = new System.Drawing.Size(26, 13);
            this.player2DeckLabel.TabIndex = 16;
            this.player2DeckLabel.Text = "P2: ";
            // 
            // player4DeckLabel
            // 
            this.player4DeckLabel.AutoSize = true;
            this.player4DeckLabel.Location = new System.Drawing.Point(3, 322);
            this.player4DeckLabel.Name = "player4DeckLabel";
            this.player4DeckLabel.Size = new System.Drawing.Size(26, 13);
            this.player4DeckLabel.TabIndex = 18;
            this.player4DeckLabel.Text = "P4: ";
            // 
            // player3DeckLabel
            // 
            this.player3DeckLabel.AutoSize = true;
            this.player3DeckLabel.Location = new System.Drawing.Point(3, 303);
            this.player3DeckLabel.Name = "player3DeckLabel";
            this.player3DeckLabel.Size = new System.Drawing.Size(26, 13);
            this.player3DeckLabel.TabIndex = 17;
            this.player3DeckLabel.Text = "P3: ";
            // 
            // startingHandNumericUpDown
            // 
            this.startingHandNumericUpDown.Location = new System.Drawing.Point(67, 240);
            this.startingHandNumericUpDown.Maximum = new decimal(new int[] {
            60000,
            0,
            0,
            0});
            this.startingHandNumericUpDown.Name = "startingHandNumericUpDown";
            this.startingHandNumericUpDown.Size = new System.Drawing.Size(120, 20);
            this.startingHandNumericUpDown.TabIndex = 20;
            this.startingHandNumericUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // startHandLabel
            // 
            this.startHandLabel.AutoSize = true;
            this.startHandLabel.Location = new System.Drawing.Point(4, 243);
            this.startHandLabel.Name = "startHandLabel";
            this.startHandLabel.Size = new System.Drawing.Size(61, 13);
            this.startHandLabel.TabIndex = 19;
            this.startHandLabel.Text = "Start Hand:";
            // 
            // startDuelButton
            // 
            this.startDuelButton.Location = new System.Drawing.Point(4, 363);
            this.startDuelButton.Name = "startDuelButton";
            this.startDuelButton.Size = new System.Drawing.Size(75, 23);
            this.startDuelButton.TabIndex = 21;
            this.startDuelButton.Text = "Start Duel";
            this.startDuelButton.UseVisualStyleBackColor = true;
            this.startDuelButton.Click += new System.EventHandler(this.startDuelButton_Click);
            // 
            // player1AICheckBox
            // 
            this.player1AICheckBox.AutoSize = true;
            this.player1AICheckBox.Location = new System.Drawing.Point(5, 341);
            this.player1AICheckBox.Name = "player1AICheckBox";
            this.player1AICheckBox.Size = new System.Drawing.Size(49, 17);
            this.player1AICheckBox.TabIndex = 22;
            this.player1AICheckBox.Text = "P1AI";
            this.player1AICheckBox.UseVisualStyleBackColor = true;
            // 
            // player2AICheckBox
            // 
            this.player2AICheckBox.AutoSize = true;
            this.player2AICheckBox.Checked = true;
            this.player2AICheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.player2AICheckBox.Location = new System.Drawing.Point(60, 341);
            this.player2AICheckBox.Name = "player2AICheckBox";
            this.player2AICheckBox.Size = new System.Drawing.Size(49, 17);
            this.player2AICheckBox.TabIndex = 23;
            this.player2AICheckBox.Text = "P2AI";
            this.player2AICheckBox.UseVisualStyleBackColor = true;
            // 
            // player3AICheckBox
            // 
            this.player3AICheckBox.AutoSize = true;
            this.player3AICheckBox.Checked = true;
            this.player3AICheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.player3AICheckBox.Location = new System.Drawing.Point(115, 341);
            this.player3AICheckBox.Name = "player3AICheckBox";
            this.player3AICheckBox.Size = new System.Drawing.Size(49, 17);
            this.player3AICheckBox.TabIndex = 24;
            this.player3AICheckBox.Text = "P3AI";
            this.player3AICheckBox.UseVisualStyleBackColor = true;
            // 
            // player4AICheckBox
            // 
            this.player4AICheckBox.AutoSize = true;
            this.player4AICheckBox.Checked = true;
            this.player4AICheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.player4AICheckBox.Location = new System.Drawing.Point(170, 340);
            this.player4AICheckBox.Name = "player4AICheckBox";
            this.player4AICheckBox.Size = new System.Drawing.Size(49, 17);
            this.player4AICheckBox.TabIndex = 25;
            this.player4AICheckBox.Text = "P4AI";
            this.player4AICheckBox.UseVisualStyleBackColor = true;
            // 
            // viewDeckDelayTimer
            // 
            this.viewDeckDelayTimer.Enabled = true;
            this.viewDeckDelayTimer.Tick += new System.EventHandler(this.viewDeckDelayTimer_Tick);
            // 
            // startingPlayerComboBox
            // 
            this.startingPlayerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.startingPlayerComboBox.FormattingEnabled = true;
            this.startingPlayerComboBox.Items.AddRange(new object[] {
            "Random",
            "Self",
            "Opponent"});
            this.startingPlayerComboBox.Location = new System.Drawing.Point(80, 164);
            this.startingPlayerComboBox.Name = "startingPlayerComboBox";
            this.startingPlayerComboBox.Size = new System.Drawing.Size(146, 21);
            this.startingPlayerComboBox.TabIndex = 26;
            // 
            // fullReloadCheckBox
            // 
            this.fullReloadCheckBox.AutoSize = true;
            this.fullReloadCheckBox.Checked = true;
            this.fullReloadCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.fullReloadCheckBox.Location = new System.Drawing.Point(5, 120);
            this.fullReloadCheckBox.Name = "fullReloadCheckBox";
            this.fullReloadCheckBox.Size = new System.Drawing.Size(79, 17);
            this.fullReloadCheckBox.TabIndex = 27;
            this.fullReloadCheckBox.Text = "Full Reload";
            this.fullReloadCheckBox.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 168);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 28;
            this.label1.Text = "Starting Player";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 194);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 29;
            this.label2.Text = "Duel Arena";
            // 
            // speedMultiplierNumericUpDown
            // 
            this.speedMultiplierNumericUpDown.DecimalPlaces = 1;
            this.speedMultiplierNumericUpDown.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.speedMultiplierNumericUpDown.Location = new System.Drawing.Point(48, 3);
            this.speedMultiplierNumericUpDown.Maximum = new decimal(new int[] {
            60000,
            0,
            0,
            0});
            this.speedMultiplierNumericUpDown.Name = "speedMultiplierNumericUpDown";
            this.speedMultiplierNumericUpDown.Size = new System.Drawing.Size(51, 20);
            this.speedMultiplierNumericUpDown.TabIndex = 30;
            this.speedMultiplierNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // speedMultiplierLabel
            // 
            this.speedMultiplierLabel.AutoSize = true;
            this.speedMultiplierLabel.Location = new System.Drawing.Point(3, 6);
            this.speedMultiplierLabel.Name = "speedMultiplierLabel";
            this.speedMultiplierLabel.Size = new System.Drawing.Size(41, 13);
            this.speedMultiplierLabel.TabIndex = 31;
            this.speedMultiplierLabel.Text = "Speed:";
            // 
            // speedMultiplierApplyButton
            // 
            this.speedMultiplierApplyButton.Location = new System.Drawing.Point(102, 1);
            this.speedMultiplierApplyButton.Name = "speedMultiplierApplyButton";
            this.speedMultiplierApplyButton.Size = new System.Drawing.Size(75, 23);
            this.speedMultiplierApplyButton.TabIndex = 32;
            this.speedMultiplierApplyButton.Text = "Apply";
            this.speedMultiplierApplyButton.UseVisualStyleBackColor = true;
            this.speedMultiplierApplyButton.Click += new System.EventHandler(this.speedMultiplierApplyButton_Click);
            // 
            // animationsButton
            // 
            this.animationsButton.Location = new System.Drawing.Point(79, 0);
            this.animationsButton.Name = "animationsButton";
            this.animationsButton.Size = new System.Drawing.Size(75, 23);
            this.animationsButton.TabIndex = 34;
            this.animationsButton.Text = "Animations";
            this.animationsButton.UseVisualStyleBackColor = true;
            this.animationsButton.Click += new System.EventHandler(this.animationsButton_Click);
            // 
            // unlockContentButtonButton
            // 
            this.unlockContentButtonButton.Location = new System.Drawing.Point(2, 0);
            this.unlockContentButtonButton.Name = "unlockContentButtonButton";
            this.unlockContentButtonButton.Size = new System.Drawing.Size(75, 23);
            this.unlockContentButtonButton.TabIndex = 35;
            this.unlockContentButtonButton.Text = "Modify Save";
            this.unlockContentButtonButton.UseVisualStyleBackColor = true;
            this.unlockContentButtonButton.Click += new System.EventHandler(this.unlockContentButtonButton_Click);
            // 
            // duelSettingsPanel
            // 
            this.duelSettingsPanel.Controls.Add(this.duelRewardsCheckBox);
            this.duelSettingsPanel.Controls.Add(this.speedMultiplierLabel);
            this.duelSettingsPanel.Controls.Add(this.tagDuelCheckBox);
            this.duelSettingsPanel.Controls.Add(this.matchDuelCheckBox);
            this.duelSettingsPanel.Controls.Add(this.player1DeckLabel);
            this.duelSettingsPanel.Controls.Add(this.speedMultiplierApplyButton);
            this.duelSettingsPanel.Controls.Add(this.duelArenaComboBox);
            this.duelSettingsPanel.Controls.Add(this.speedDuelCheckBox);
            this.duelSettingsPanel.Controls.Add(this.speedMultiplierNumericUpDown);
            this.duelSettingsPanel.Controls.Add(this.lifePointsLabel);
            this.duelSettingsPanel.Controls.Add(this.label2);
            this.duelSettingsPanel.Controls.Add(this.lifePointsNumericUpDown);
            this.duelSettingsPanel.Controls.Add(this.label1);
            this.duelSettingsPanel.Controls.Add(this.skipRockPaperScissorsCheckBox);
            this.duelSettingsPanel.Controls.Add(this.fullReloadCheckBox);
            this.duelSettingsPanel.Controls.Add(this.player2DeckLabel);
            this.duelSettingsPanel.Controls.Add(this.startingPlayerComboBox);
            this.duelSettingsPanel.Controls.Add(this.player3DeckLabel);
            this.duelSettingsPanel.Controls.Add(this.player4AICheckBox);
            this.duelSettingsPanel.Controls.Add(this.player4DeckLabel);
            this.duelSettingsPanel.Controls.Add(this.player3AICheckBox);
            this.duelSettingsPanel.Controls.Add(this.startHandLabel);
            this.duelSettingsPanel.Controls.Add(this.player2AICheckBox);
            this.duelSettingsPanel.Controls.Add(this.startingHandNumericUpDown);
            this.duelSettingsPanel.Controls.Add(this.player1AICheckBox);
            this.duelSettingsPanel.Controls.Add(this.startDuelButton);
            this.duelSettingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.duelSettingsPanel.Location = new System.Drawing.Point(217, 25);
            this.duelSettingsPanel.Name = "duelSettingsPanel";
            this.duelSettingsPanel.Size = new System.Drawing.Size(278, 425);
            this.duelSettingsPanel.TabIndex = 36;
            // 
            // duelRewardsCheckBox
            // 
            this.duelRewardsCheckBox.AutoSize = true;
            this.duelRewardsCheckBox.Checked = true;
            this.duelRewardsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.duelRewardsCheckBox.Location = new System.Drawing.Point(5, 143);
            this.duelRewardsCheckBox.Name = "duelRewardsCheckBox";
            this.duelRewardsCheckBox.Size = new System.Drawing.Size(68, 17);
            this.duelRewardsCheckBox.TabIndex = 33;
            this.duelRewardsCheckBox.Text = "Rewards";
            this.duelRewardsCheckBox.UseVisualStyleBackColor = true;
            // 
            // setsDecksPanel
            // 
            this.setsDecksPanel.Controls.Add(this.animationsButton);
            this.setsDecksPanel.Controls.Add(this.unlockContentButtonButton);
            this.setsDecksPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.setsDecksPanel.Location = new System.Drawing.Point(217, 0);
            this.setsDecksPanel.Name = "setsDecksPanel";
            this.setsDecksPanel.Size = new System.Drawing.Size(278, 25);
            this.setsDecksPanel.TabIndex = 37;
            // 
            // DuelStarterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(495, 450);
            this.Controls.Add(this.duelSettingsPanel);
            this.Controls.Add(this.setsDecksPanel);
            this.Controls.Add(this.decksListPanel1);
            this.Name = "DuelStarterForm";
            this.Text = "LOTD - Duel Starter";
            this.decksListPanel1.ResumeLayout(false);
            this.decksListPanel1.PerformLayout();
            this.playerDeckButtonsPanel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.deckFilterPanel.ResumeLayout(false);
            this.deckFilterPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.lifePointsNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.startingHandNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.speedMultiplierNumericUpDown)).EndInit();
            this.duelSettingsPanel.ResumeLayout(false);
            this.duelSettingsPanel.PerformLayout();
            this.setsDecksPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.CheckBox tagDuelCheckBox;
        private System.Windows.Forms.CheckBox matchDuelCheckBox;
        private System.Windows.Forms.ListBox decksListBox;
        private System.Windows.Forms.Panel decksListPanel1;
        private System.Windows.Forms.Button viewDeckButton;
        private System.Windows.Forms.ComboBox deckTypeComboBox;
        private System.Windows.Forms.Label player1DeckLabel;
        private System.Windows.Forms.ComboBox duelArenaComboBox;
        private System.Windows.Forms.CheckBox autoViewDeckCheckBox;
        private System.Windows.Forms.CheckBox speedDuelCheckBox;
        private System.Windows.Forms.Label lifePointsLabel;
        private System.Windows.Forms.NumericUpDown lifePointsNumericUpDown;
        private System.Windows.Forms.CheckBox skipRockPaperScissorsCheckBox;
        private System.Windows.Forms.Panel playerDeckButtonsPanel;
        private System.Windows.Forms.Button setPlayer4Button;
        private System.Windows.Forms.Button setPlayer3Button;
        private System.Windows.Forms.Button setPlayer2DeckButton;
        private System.Windows.Forms.Button setPlayer1DeckButton;
        private System.Windows.Forms.Label player2DeckLabel;
        private System.Windows.Forms.Label player4DeckLabel;
        private System.Windows.Forms.Label player3DeckLabel;
        private System.Windows.Forms.NumericUpDown startingHandNumericUpDown;
        private System.Windows.Forms.Label startHandLabel;
        private System.Windows.Forms.Button startDuelButton;
        private System.Windows.Forms.Button exportDeckButton;
        private System.Windows.Forms.Button reloadDecksButton;
        private System.Windows.Forms.CheckBox player1AICheckBox;
        private System.Windows.Forms.CheckBox player2AICheckBox;
        private System.Windows.Forms.CheckBox player3AICheckBox;
        private System.Windows.Forms.CheckBox player4AICheckBox;
        private System.Windows.Forms.Timer viewDeckDelayTimer;
        private System.Windows.Forms.ComboBox startingPlayerComboBox;
        private System.Windows.Forms.Button clearPlayerDecksButton;
        private System.Windows.Forms.CheckBox fullReloadCheckBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown speedMultiplierNumericUpDown;
        private System.Windows.Forms.Label speedMultiplierLabel;
        private System.Windows.Forms.Button speedMultiplierApplyButton;
        private System.Windows.Forms.Button animationsButton;
        private System.Windows.Forms.Button unlockContentButtonButton;
        private System.Windows.Forms.Panel duelSettingsPanel;
        private System.Windows.Forms.Panel setsDecksPanel;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox deckFilterTextBox;
        private System.Windows.Forms.CheckBox duelRewardsCheckBox;
        private System.Windows.Forms.Panel deckFilterPanel;
        private System.Windows.Forms.CheckBox filterDeckRitualCheckBox;
        private System.Windows.Forms.CheckBox filterDeckXyzCheckBox;
        private System.Windows.Forms.CheckBox filterDeckFusionCheckBox;
        private System.Windows.Forms.CheckBox filterDeckSynchroCheckBox;
        private System.Windows.Forms.CheckBox filterDeckPendulumCheckBox;
    }
}