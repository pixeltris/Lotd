namespace Lotd.UI
{
    partial class AnimationsForm
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
            this.actionsAnimationsSplitContainer = new System.Windows.Forms.SplitContainer();
            this.actionsSplitContainer = new System.Windows.Forms.SplitContainer();
            this.actionsDataGridView = new System.Windows.Forms.DataGridView();
            this.unblockedActionTimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unblockedActionValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unblockedActionNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.actionPanel1 = new System.Windows.Forms.Panel();
            this.showBlockedActionsCheckBox = new System.Windows.Forms.CheckBox();
            this.logActionsCheckBox = new System.Windows.Forms.CheckBox();
            this.blockActionButton = new System.Windows.Forms.Button();
            this.blockedActionsDataGridView = new System.Windows.Forms.DataGridView();
            this.blockedActionValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.blockedActionNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.actionPanel2 = new System.Windows.Forms.Panel();
            this.unblockActionButton = new System.Windows.Forms.Button();
            this.animationsSplitContainer = new System.Windows.Forms.SplitContainer();
            this.animationsDataGridView = new System.Windows.Forms.DataGridView();
            this.unblockedAnimationTimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unblockedAnimationValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unblockedAnimationNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.animationPanel1 = new System.Windows.Forms.Panel();
            this.showBlockedAnimationsCheckBox = new System.Windows.Forms.CheckBox();
            this.logAnimationsCheckBox = new System.Windows.Forms.CheckBox();
            this.blockAnimationButton = new System.Windows.Forms.Button();
            this.blockedAnimationsDataGridView = new System.Windows.Forms.DataGridView();
            this.blockedAnimationValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.blockedAnimationNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.animationPanel2 = new System.Windows.Forms.Panel();
            this.unblockAnimationButton = new System.Windows.Forms.Button();
            this.updateTimer = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.clearLogsButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.actionsAnimationsSplitContainer)).BeginInit();
            this.actionsAnimationsSplitContainer.Panel1.SuspendLayout();
            this.actionsAnimationsSplitContainer.Panel2.SuspendLayout();
            this.actionsAnimationsSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.actionsSplitContainer)).BeginInit();
            this.actionsSplitContainer.Panel1.SuspendLayout();
            this.actionsSplitContainer.Panel2.SuspendLayout();
            this.actionsSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.actionsDataGridView)).BeginInit();
            this.actionPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.blockedActionsDataGridView)).BeginInit();
            this.actionPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationsSplitContainer)).BeginInit();
            this.animationsSplitContainer.Panel1.SuspendLayout();
            this.animationsSplitContainer.Panel2.SuspendLayout();
            this.animationsSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.animationsDataGridView)).BeginInit();
            this.animationPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.blockedAnimationsDataGridView)).BeginInit();
            this.animationPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // actionsAnimationsSplitContainer
            // 
            this.actionsAnimationsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionsAnimationsSplitContainer.Location = new System.Drawing.Point(0, 25);
            this.actionsAnimationsSplitContainer.Name = "actionsAnimationsSplitContainer";
            // 
            // actionsAnimationsSplitContainer.Panel1
            // 
            this.actionsAnimationsSplitContainer.Panel1.Controls.Add(this.actionsSplitContainer);
            // 
            // actionsAnimationsSplitContainer.Panel2
            // 
            this.actionsAnimationsSplitContainer.Panel2.Controls.Add(this.animationsSplitContainer);
            this.actionsAnimationsSplitContainer.Size = new System.Drawing.Size(554, 396);
            this.actionsAnimationsSplitContainer.SplitterDistance = 273;
            this.actionsAnimationsSplitContainer.TabIndex = 0;
            // 
            // actionsSplitContainer
            // 
            this.actionsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionsSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.actionsSplitContainer.Name = "actionsSplitContainer";
            this.actionsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // actionsSplitContainer.Panel1
            // 
            this.actionsSplitContainer.Panel1.Controls.Add(this.actionsDataGridView);
            this.actionsSplitContainer.Panel1.Controls.Add(this.actionPanel1);
            // 
            // actionsSplitContainer.Panel2
            // 
            this.actionsSplitContainer.Panel2.Controls.Add(this.blockedActionsDataGridView);
            this.actionsSplitContainer.Panel2.Controls.Add(this.actionPanel2);
            this.actionsSplitContainer.Size = new System.Drawing.Size(273, 396);
            this.actionsSplitContainer.SplitterDistance = 195;
            this.actionsSplitContainer.TabIndex = 0;
            // 
            // actionsDataGridView
            // 
            this.actionsDataGridView.AllowUserToAddRows = false;
            this.actionsDataGridView.AllowUserToDeleteRows = false;
            this.actionsDataGridView.AllowUserToResizeRows = false;
            this.actionsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.actionsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.unblockedActionTimeColumn,
            this.unblockedActionValueColumn,
            this.unblockedActionNameColumn});
            this.actionsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionsDataGridView.Location = new System.Drawing.Point(0, 25);
            this.actionsDataGridView.Name = "actionsDataGridView";
            this.actionsDataGridView.RowHeadersVisible = false;
            this.actionsDataGridView.Size = new System.Drawing.Size(273, 170);
            this.actionsDataGridView.TabIndex = 1;
            // 
            // unblockedActionTimeColumn
            // 
            this.unblockedActionTimeColumn.HeaderText = "Time";
            this.unblockedActionTimeColumn.Name = "unblockedActionTimeColumn";
            this.unblockedActionTimeColumn.ReadOnly = true;
            this.unblockedActionTimeColumn.Width = 70;
            // 
            // unblockedActionValueColumn
            // 
            this.unblockedActionValueColumn.HeaderText = "Value";
            this.unblockedActionValueColumn.Name = "unblockedActionValueColumn";
            this.unblockedActionValueColumn.ReadOnly = true;
            this.unblockedActionValueColumn.Width = 45;
            // 
            // unblockedActionNameColumn
            // 
            this.unblockedActionNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.unblockedActionNameColumn.HeaderText = "Name";
            this.unblockedActionNameColumn.Name = "unblockedActionNameColumn";
            this.unblockedActionNameColumn.ReadOnly = true;
            // 
            // actionPanel1
            // 
            this.actionPanel1.Controls.Add(this.showBlockedActionsCheckBox);
            this.actionPanel1.Controls.Add(this.logActionsCheckBox);
            this.actionPanel1.Controls.Add(this.blockActionButton);
            this.actionPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.actionPanel1.Location = new System.Drawing.Point(0, 0);
            this.actionPanel1.Name = "actionPanel1";
            this.actionPanel1.Size = new System.Drawing.Size(273, 25);
            this.actionPanel1.TabIndex = 0;
            // 
            // showBlockedActionsCheckBox
            // 
            this.showBlockedActionsCheckBox.AutoSize = true;
            this.showBlockedActionsCheckBox.Checked = true;
            this.showBlockedActionsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showBlockedActionsCheckBox.Location = new System.Drawing.Point(152, 5);
            this.showBlockedActionsCheckBox.Name = "showBlockedActionsCheckBox";
            this.showBlockedActionsCheckBox.Size = new System.Drawing.Size(95, 17);
            this.showBlockedActionsCheckBox.TabIndex = 2;
            this.showBlockedActionsCheckBox.Text = "Show Blocked";
            this.showBlockedActionsCheckBox.UseVisualStyleBackColor = true;
            // 
            // logActionsCheckBox
            // 
            this.logActionsCheckBox.AutoSize = true;
            this.logActionsCheckBox.Location = new System.Drawing.Point(102, 5);
            this.logActionsCheckBox.Name = "logActionsCheckBox";
            this.logActionsCheckBox.Size = new System.Drawing.Size(44, 17);
            this.logActionsCheckBox.TabIndex = 1;
            this.logActionsCheckBox.Text = "Log";
            this.logActionsCheckBox.UseVisualStyleBackColor = true;
            // 
            // blockActionButton
            // 
            this.blockActionButton.Location = new System.Drawing.Point(1, 1);
            this.blockActionButton.Name = "blockActionButton";
            this.blockActionButton.Size = new System.Drawing.Size(95, 23);
            this.blockActionButton.TabIndex = 0;
            this.blockActionButton.Text = "Block Action";
            this.blockActionButton.UseVisualStyleBackColor = true;
            this.blockActionButton.Click += new System.EventHandler(this.blockActionButton_Click);
            // 
            // blockedActionsDataGridView
            // 
            this.blockedActionsDataGridView.AllowUserToAddRows = false;
            this.blockedActionsDataGridView.AllowUserToDeleteRows = false;
            this.blockedActionsDataGridView.AllowUserToResizeRows = false;
            this.blockedActionsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.blockedActionsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.blockedActionValueColumn,
            this.blockedActionNameColumn});
            this.blockedActionsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.blockedActionsDataGridView.Location = new System.Drawing.Point(0, 25);
            this.blockedActionsDataGridView.Name = "blockedActionsDataGridView";
            this.blockedActionsDataGridView.RowHeadersVisible = false;
            this.blockedActionsDataGridView.Size = new System.Drawing.Size(273, 172);
            this.blockedActionsDataGridView.TabIndex = 3;
            // 
            // blockedActionValueColumn
            // 
            this.blockedActionValueColumn.HeaderText = "Value";
            this.blockedActionValueColumn.Name = "blockedActionValueColumn";
            this.blockedActionValueColumn.ReadOnly = true;
            this.blockedActionValueColumn.Width = 45;
            // 
            // blockedActionNameColumn
            // 
            this.blockedActionNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.blockedActionNameColumn.HeaderText = "Name";
            this.blockedActionNameColumn.Name = "blockedActionNameColumn";
            this.blockedActionNameColumn.ReadOnly = true;
            // 
            // actionPanel2
            // 
            this.actionPanel2.Controls.Add(this.unblockActionButton);
            this.actionPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.actionPanel2.Location = new System.Drawing.Point(0, 0);
            this.actionPanel2.Name = "actionPanel2";
            this.actionPanel2.Size = new System.Drawing.Size(273, 25);
            this.actionPanel2.TabIndex = 2;
            // 
            // unblockActionButton
            // 
            this.unblockActionButton.Location = new System.Drawing.Point(1, 1);
            this.unblockActionButton.Name = "unblockActionButton";
            this.unblockActionButton.Size = new System.Drawing.Size(95, 23);
            this.unblockActionButton.TabIndex = 0;
            this.unblockActionButton.Text = "Unblock Action";
            this.unblockActionButton.UseVisualStyleBackColor = true;
            this.unblockActionButton.Click += new System.EventHandler(this.unblockActionButton_Click);
            // 
            // animationsSplitContainer
            // 
            this.animationsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationsSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.animationsSplitContainer.Name = "animationsSplitContainer";
            this.animationsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // animationsSplitContainer.Panel1
            // 
            this.animationsSplitContainer.Panel1.Controls.Add(this.animationsDataGridView);
            this.animationsSplitContainer.Panel1.Controls.Add(this.animationPanel1);
            // 
            // animationsSplitContainer.Panel2
            // 
            this.animationsSplitContainer.Panel2.Controls.Add(this.blockedAnimationsDataGridView);
            this.animationsSplitContainer.Panel2.Controls.Add(this.animationPanel2);
            this.animationsSplitContainer.Size = new System.Drawing.Size(277, 396);
            this.animationsSplitContainer.SplitterDistance = 195;
            this.animationsSplitContainer.TabIndex = 1;
            // 
            // animationsDataGridView
            // 
            this.animationsDataGridView.AllowUserToAddRows = false;
            this.animationsDataGridView.AllowUserToDeleteRows = false;
            this.animationsDataGridView.AllowUserToResizeRows = false;
            this.animationsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.animationsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.unblockedAnimationTimeColumn,
            this.unblockedAnimationValueColumn,
            this.unblockedAnimationNameColumn});
            this.animationsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animationsDataGridView.Location = new System.Drawing.Point(0, 25);
            this.animationsDataGridView.Name = "animationsDataGridView";
            this.animationsDataGridView.RowHeadersVisible = false;
            this.animationsDataGridView.Size = new System.Drawing.Size(277, 170);
            this.animationsDataGridView.TabIndex = 2;
            // 
            // unblockedAnimationTimeColumn
            // 
            this.unblockedAnimationTimeColumn.HeaderText = "Time";
            this.unblockedAnimationTimeColumn.Name = "unblockedAnimationTimeColumn";
            this.unblockedAnimationTimeColumn.ReadOnly = true;
            this.unblockedAnimationTimeColumn.Width = 70;
            // 
            // unblockedAnimationValueColumn
            // 
            this.unblockedAnimationValueColumn.HeaderText = "Value";
            this.unblockedAnimationValueColumn.Name = "unblockedAnimationValueColumn";
            this.unblockedAnimationValueColumn.ReadOnly = true;
            this.unblockedAnimationValueColumn.Width = 45;
            // 
            // unblockedAnimationNameColumn
            // 
            this.unblockedAnimationNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.unblockedAnimationNameColumn.HeaderText = "Name";
            this.unblockedAnimationNameColumn.Name = "unblockedAnimationNameColumn";
            this.unblockedAnimationNameColumn.ReadOnly = true;
            // 
            // animationPanel1
            // 
            this.animationPanel1.Controls.Add(this.showBlockedAnimationsCheckBox);
            this.animationPanel1.Controls.Add(this.logAnimationsCheckBox);
            this.animationPanel1.Controls.Add(this.blockAnimationButton);
            this.animationPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.animationPanel1.Location = new System.Drawing.Point(0, 0);
            this.animationPanel1.Name = "animationPanel1";
            this.animationPanel1.Size = new System.Drawing.Size(277, 25);
            this.animationPanel1.TabIndex = 1;
            // 
            // showBlockedAnimationsCheckBox
            // 
            this.showBlockedAnimationsCheckBox.AutoSize = true;
            this.showBlockedAnimationsCheckBox.Checked = true;
            this.showBlockedAnimationsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showBlockedAnimationsCheckBox.Location = new System.Drawing.Point(167, 5);
            this.showBlockedAnimationsCheckBox.Name = "showBlockedAnimationsCheckBox";
            this.showBlockedAnimationsCheckBox.Size = new System.Drawing.Size(95, 17);
            this.showBlockedAnimationsCheckBox.TabIndex = 2;
            this.showBlockedAnimationsCheckBox.Text = "Show Blocked";
            this.showBlockedAnimationsCheckBox.UseVisualStyleBackColor = true;
            // 
            // logAnimationsCheckBox
            // 
            this.logAnimationsCheckBox.AutoSize = true;
            this.logAnimationsCheckBox.Location = new System.Drawing.Point(117, 5);
            this.logAnimationsCheckBox.Name = "logAnimationsCheckBox";
            this.logAnimationsCheckBox.Size = new System.Drawing.Size(44, 17);
            this.logAnimationsCheckBox.TabIndex = 1;
            this.logAnimationsCheckBox.Text = "Log";
            this.logAnimationsCheckBox.UseVisualStyleBackColor = true;
            // 
            // blockAnimationButton
            // 
            this.blockAnimationButton.Location = new System.Drawing.Point(1, 1);
            this.blockAnimationButton.Name = "blockAnimationButton";
            this.blockAnimationButton.Size = new System.Drawing.Size(110, 23);
            this.blockAnimationButton.TabIndex = 0;
            this.blockAnimationButton.Text = "Block Animation";
            this.blockAnimationButton.UseVisualStyleBackColor = true;
            this.blockAnimationButton.Click += new System.EventHandler(this.blockAnimationButton_Click);
            // 
            // blockedAnimationsDataGridView
            // 
            this.blockedAnimationsDataGridView.AllowUserToAddRows = false;
            this.blockedAnimationsDataGridView.AllowUserToDeleteRows = false;
            this.blockedAnimationsDataGridView.AllowUserToResizeRows = false;
            this.blockedAnimationsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.blockedAnimationsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.blockedAnimationValueColumn,
            this.blockedAnimationNameColumn});
            this.blockedAnimationsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.blockedAnimationsDataGridView.Location = new System.Drawing.Point(0, 25);
            this.blockedAnimationsDataGridView.Name = "blockedAnimationsDataGridView";
            this.blockedAnimationsDataGridView.RowHeadersVisible = false;
            this.blockedAnimationsDataGridView.Size = new System.Drawing.Size(277, 172);
            this.blockedAnimationsDataGridView.TabIndex = 4;
            // 
            // blockedAnimationValueColumn
            // 
            this.blockedAnimationValueColumn.HeaderText = "Value";
            this.blockedAnimationValueColumn.Name = "blockedAnimationValueColumn";
            this.blockedAnimationValueColumn.ReadOnly = true;
            this.blockedAnimationValueColumn.Width = 45;
            // 
            // blockedAnimationNameColumn
            // 
            this.blockedAnimationNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.blockedAnimationNameColumn.HeaderText = "Name";
            this.blockedAnimationNameColumn.Name = "blockedAnimationNameColumn";
            this.blockedAnimationNameColumn.ReadOnly = true;
            // 
            // animationPanel2
            // 
            this.animationPanel2.Controls.Add(this.unblockAnimationButton);
            this.animationPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.animationPanel2.Location = new System.Drawing.Point(0, 0);
            this.animationPanel2.Name = "animationPanel2";
            this.animationPanel2.Size = new System.Drawing.Size(277, 25);
            this.animationPanel2.TabIndex = 3;
            // 
            // unblockAnimationButton
            // 
            this.unblockAnimationButton.Location = new System.Drawing.Point(1, 1);
            this.unblockAnimationButton.Name = "unblockAnimationButton";
            this.unblockAnimationButton.Size = new System.Drawing.Size(110, 23);
            this.unblockAnimationButton.TabIndex = 0;
            this.unblockAnimationButton.Text = "Unblock Animation";
            this.unblockAnimationButton.UseVisualStyleBackColor = true;
            this.unblockAnimationButton.Click += new System.EventHandler(this.unblockAnimationButton_Click);
            // 
            // updateTimer
            // 
            this.updateTimer.Enabled = true;
            this.updateTimer.Interval = 50;
            this.updateTimer.Tick += new System.EventHandler(this.updateTimer_Tick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.clearLogsButton);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.numericUpDown1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(554, 25);
            this.panel1.TabIndex = 1;
            // 
            // clearLogsButton
            // 
            this.clearLogsButton.Location = new System.Drawing.Point(136, 1);
            this.clearLogsButton.Name = "clearLogsButton";
            this.clearLogsButton.Size = new System.Drawing.Size(75, 23);
            this.clearLogsButton.TabIndex = 2;
            this.clearLogsButton.Text = "Clear Logs";
            this.clearLogsButton.UseVisualStyleBackColor = true;
            this.clearLogsButton.Click += new System.EventHandler(this.clearLogsButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Log Tick:";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(59, 3);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(68, 20);
            this.numericUpDown1.TabIndex = 0;
            this.numericUpDown1.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // AnimationsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(554, 421);
            this.Controls.Add(this.actionsAnimationsSplitContainer);
            this.Controls.Add(this.panel1);
            this.Name = "AnimationsForm";
            this.Text = "Duel Animations";
            this.actionsAnimationsSplitContainer.Panel1.ResumeLayout(false);
            this.actionsAnimationsSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.actionsAnimationsSplitContainer)).EndInit();
            this.actionsAnimationsSplitContainer.ResumeLayout(false);
            this.actionsSplitContainer.Panel1.ResumeLayout(false);
            this.actionsSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.actionsSplitContainer)).EndInit();
            this.actionsSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.actionsDataGridView)).EndInit();
            this.actionPanel1.ResumeLayout(false);
            this.actionPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.blockedActionsDataGridView)).EndInit();
            this.actionPanel2.ResumeLayout(false);
            this.animationsSplitContainer.Panel1.ResumeLayout(false);
            this.animationsSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.animationsSplitContainer)).EndInit();
            this.animationsSplitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.animationsDataGridView)).EndInit();
            this.animationPanel1.ResumeLayout(false);
            this.animationPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.blockedAnimationsDataGridView)).EndInit();
            this.animationPanel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer actionsAnimationsSplitContainer;
        private System.Windows.Forms.SplitContainer actionsSplitContainer;
        private System.Windows.Forms.SplitContainer animationsSplitContainer;
        private System.Windows.Forms.Panel actionPanel1;
        private System.Windows.Forms.CheckBox logActionsCheckBox;
        private System.Windows.Forms.Button blockActionButton;
        private System.Windows.Forms.Panel actionPanel2;
        private System.Windows.Forms.Button unblockActionButton;
        private System.Windows.Forms.Panel animationPanel1;
        private System.Windows.Forms.CheckBox logAnimationsCheckBox;
        private System.Windows.Forms.Button blockAnimationButton;
        private System.Windows.Forms.Panel animationPanel2;
        private System.Windows.Forms.Button unblockAnimationButton;
        private System.Windows.Forms.DataGridView actionsDataGridView;
        private System.Windows.Forms.DataGridView blockedActionsDataGridView;
        private System.Windows.Forms.DataGridView animationsDataGridView;
        private System.Windows.Forms.DataGridView blockedAnimationsDataGridView;
        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.CheckBox showBlockedActionsCheckBox;
        private System.Windows.Forms.CheckBox showBlockedAnimationsCheckBox;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.DataGridViewTextBoxColumn unblockedActionTimeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn unblockedActionValueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn unblockedActionNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn blockedActionValueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn blockedActionNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn unblockedAnimationTimeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn unblockedAnimationValueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn unblockedAnimationNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn blockedAnimationValueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn blockedAnimationNameColumn;
        private System.Windows.Forms.Button clearLogsButton;
    }
}