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
    public partial class AnimationsForm : Form
    {
        private string file = "BlockedAnimations.txt";
        private string animationNamesFile = "AnimationNames.txt";
        private Dictionary<int, string> actionIdNames = new Dictionary<int, string>();
        private Dictionary<int, string> animationIdNames = new Dictionary<int, string>();
        private HashSet<int> blockedActionIds = new HashSet<int>();
        private HashSet<int> blockedAnimationIds = new HashSet<int>();
        private int logLimit = 100;

        private int lastActionId = 0;
        private int lastAnimationId = 0;

        public AnimationsForm()
        {
            InitializeComponent();

            LoadBlockedAnimations();
            UpdateBlockList(false);
            UpdateBlockList(true);
        }

        private void LoadBlockedAnimations()
        {
            try
            {
                blockedActionIds.Clear();
                blockedAnimationIds.Clear();

                if (File.Exists(file))
                {
                    bool isActionIds = false;
                    bool isAnimationIds = false;

                    string[] lines = File.ReadAllLines(file);
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        string trimmed = line.Trim();
                        if (trimmed.StartsWith("["))
                        {
                            isActionIds = false;
                            isAnimationIds = false;
                            switch (trimmed.ToLower())
                            {
                                case "[actions]":
                                    isActionIds = true;
                                    break;
                                case "[animations]":
                                    isAnimationIds = true;
                                    break;
                            }
                        }
                        else
                        {
                            int value;
                            if (int.TryParse(trimmed, out value))
                            {
                                if (isActionIds)
                                {
                                    blockedActionIds.Add(value);
                                }
                                else if (isAnimationIds)
                                {
                                    blockedAnimationIds.Add(value);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }

            LoadNames();

            ActionId[] currentBlockedActionIds = Program.MemTools.GetBlockedActionIds();
            if (currentBlockedActionIds == null || currentBlockedActionIds.Length != blockedActionIds.Count)
            {
                Program.MemTools.ClearBlockedActionIds();
                Program.MemTools.BlockActionIds(blockedActionIds.ToArray());
            }

            byte[] currentBlockedAnimationIds = Program.MemTools.GetBlockedAnimationIds();
            if (currentBlockedAnimationIds == null || currentBlockedAnimationIds.Length != blockedAnimationIds.Count)
            {
                Program.MemTools.ClearBlockedAnimationIds();
                Program.MemTools.BlockAnimationIds(blockedAnimationIds.ToArray());
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            LoadBlockedAnimations();
            UpdateBlockList(false);
            UpdateBlockList(true);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveBlockedIds();
            e.Cancel = true;
            Hide();
            base.OnClosing(e);
        }

        private void SaveBlockedIds()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("[actions]");
            foreach (int actionId in blockedActionIds.OrderBy(x => x))
            {
                stringBuilder.AppendLine(actionId.ToString());
            }

            stringBuilder.AppendLine("[animations]");
            foreach (int animationId in blockedAnimationIds.OrderBy(x => x))
            {
                stringBuilder.AppendLine(animationId.ToString());
            }

            try
            {
                File.WriteAllText(file, stringBuilder.ToString());
            }
            catch
            {
            }
        }

        private void LoadNames()
        {
            actionIdNames.Clear();
            animationIdNames.Clear();

            try
            {
                if (File.Exists(animationNamesFile))
                {
                    bool isActionIds = false;
                    bool isAnimationIds = false;

                    string[] lines = File.ReadAllLines(animationNamesFile);
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        string trimmed = line.Trim();
                        if (trimmed.StartsWith("["))
                        {
                            isActionIds = false;
                            isAnimationIds = false;
                            switch (trimmed.ToLower())
                            {
                                case "[actions]":
                                    isActionIds = true;
                                    break;
                                case "[animations]":
                                    isAnimationIds = true;
                                    break;
                            }
                        }
                        else
                        {
                            int firstSpace = trimmed.IndexOf(' ');
                            if (firstSpace > 0)
                            {
                                string name = trimmed.Substring(firstSpace + 1);

                                int value;
                                if (int.TryParse(trimmed.Substring(0, firstSpace), out value) &&
                                    !string.IsNullOrEmpty(name))
                                {
                                    if (isActionIds)
                                    {
                                        actionIdNames[value] = name;
                                    }
                                    else if (isAnimationIds)
                                    {
                                        animationIdNames[value] = name;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private void UpdateBlockList(bool animations)
        {
            if (animations)
            {
                Program.MemTools.ClearBlockedAnimationIds();

                blockedAnimationsDataGridView.Rows.Clear();
                foreach (int animationId in blockedAnimationIds.OrderBy(x => x))
                {
                    string name;
                    animationIdNames.TryGetValue(animationId, out name);
                    blockedAnimationsDataGridView.Rows.Add(animationId, name);
                }

                Program.MemTools.BlockAnimationIds(blockedAnimationIds.ToArray());
            }
            else
            {
                Program.MemTools.ClearBlockedActionIds();

                blockedActionsDataGridView.Rows.Clear();
                foreach (int actionId in blockedActionIds.OrderBy(x => x))
                {
                    string name;
                    actionIdNames.TryGetValue(actionId, out name);
                    blockedActionsDataGridView.Rows.Add(actionId, name);
                }

                Program.MemTools.BlockActionIds(blockedActionIds.ToArray());
            }
        }

        private void clearLogsButton_Click(object sender, EventArgs e)
        {
            actionsDataGridView.Rows.Clear();
            animationsDataGridView.Rows.Clear();
        }

        private void blockActionButton_Click(object sender, EventArgs e)
        {
            bool hasBlockListChanged = false;
            foreach (int actionId in GetSelectedIds(actionsDataGridView))
            {
                if (blockedActionIds.Add(actionId))
                {
                    hasBlockListChanged = true;
                }
            }

            if (hasBlockListChanged)
            {
                UpdateBlockList(false);
                SaveBlockedIds();
            }
        }

        private void blockAnimationButton_Click(object sender, EventArgs e)
        {
            bool hasBlockListChanged = false;
            foreach (int animationId in GetSelectedIds(animationsDataGridView))
            {
                if (blockedAnimationIds.Add(animationId))
                {
                    hasBlockListChanged = true;
                }
            }

            if (hasBlockListChanged)
            {
                UpdateBlockList(true);
                SaveBlockedIds();
            }
        }

        private void unblockActionButton_Click(object sender, EventArgs e)
        {
            bool hasBlockListChanged = false;
            int cellIndex = GetValueCellIndex(blockedActionsDataGridView);
            foreach (DataGridViewRow row in blockedActionsDataGridView.GetSelectedRows())
            {
                int actionId = (int)row.Cells[cellIndex].Value;
                if (blockedActionIds.Contains(actionId))
                {
                    blockedActionIds.Remove(actionId);
                    hasBlockListChanged = true;
                }
            }

            if (hasBlockListChanged)
            {
                UpdateBlockList(false);
                SaveBlockedIds();
            }
        }

        private void unblockAnimationButton_Click(object sender, EventArgs e)
        {
            bool hasBlockListChanged = false;
            int cellIndex = GetValueCellIndex(blockedAnimationsDataGridView);
            foreach (DataGridViewRow row in blockedAnimationsDataGridView.GetSelectedRows())
            {
                int animationId = (int)row.Cells[cellIndex].Value;
                if (blockedAnimationIds.Contains(animationId))
                {
                    blockedAnimationIds.Remove(animationId);
                    hasBlockListChanged = true;
                }
            }

            if (hasBlockListChanged)
            {
                UpdateBlockList(true);
                SaveBlockedIds();
            }
        }

        private int GetSelectedId(DataGridView dataGridView)
        {
            int[] ids = GetSelectedIds(dataGridView);
            return ids.Length > 0 ? ids[0] : -1;
        }

        private int[] GetSelectedIds(DataGridView dataGridView)
        {
            int cellIndex = GetValueCellIndex(dataGridView);
            List<int> ids = new List<int>();
            DataGridViewRow[] rows = dataGridView.GetSelectedRows();
            for (int i = 0; i < rows.Length; i++)
            {
                ids.Add((int)rows[i].Cells[cellIndex].Value);
            }
            return ids.ToArray();
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            updateTimer.Interval = (int)Math.Max(1, numericUpDown1.Value);

            if (!Visible)
            {
                return;
            }

            int currentActionId = Program.MemTools.GetCurrentActionId();
            int currentAnimationId = Program.MemTools.GetCurrentAnimationId();

            if (logActionsCheckBox.Checked && currentActionId != lastActionId)
            {
                if (showBlockedActionsCheckBox.Checked || !blockedActionIds.Contains(currentActionId))
                {
                    bool isLastRowVisible = false;
                    if (actionsDataGridView.Rows.Count > 0)
                    {
                        isLastRowVisible = actionsDataGridView.Rows[actionsDataGridView.Rows.Count - 1].Cells[0].Displayed;
                    }

                    string name;
                    actionIdNames.TryGetValue(currentActionId, out name);

                    actionsDataGridView.Rows.Add(FormatTime(DateTime.Now.TimeOfDay), currentActionId, name);
                    while (actionsDataGridView.Rows.Count > logLimit)
                    {
                        actionsDataGridView.Rows.RemoveAt(0);
                    }

                    if (isLastRowVisible && actionsDataGridView.Rows.Count > 0)
                    {
                        actionsDataGridView.FirstDisplayedScrollingRowIndex = actionsDataGridView.Rows.Count - 1;
                    }
                }
                lastActionId = currentActionId;
            }
            if (logAnimationsCheckBox.Checked && currentAnimationId != lastAnimationId)
            {
                if (showBlockedAnimationsCheckBox.Checked || !blockedAnimationIds.Contains(currentAnimationId))
                {
                    bool isLastRowVisible = false;
                    if (animationsDataGridView.Rows.Count > 0)
                    {
                        isLastRowVisible = animationsDataGridView.Rows[animationsDataGridView.Rows.Count - 1].Cells[0].Displayed;
                    }

                    string name;
                    animationIdNames.TryGetValue(currentAnimationId, out name);

                    animationsDataGridView.Rows.Add(FormatTime(DateTime.Now.TimeOfDay), currentAnimationId, name);
                    while (animationsDataGridView.Rows.Count > logLimit)
                    {
                        animationsDataGridView.Rows.RemoveAt(0);
                    }

                    if (isLastRowVisible && animationsDataGridView.Rows.Count > 0)
                    {
                        animationsDataGridView.FirstDisplayedScrollingRowIndex = animationsDataGridView.Rows.Count - 1;
                    }
                }
                lastAnimationId = currentAnimationId;
            }
        }

        private string FormatTime(TimeSpan time)
        {
            return
                time.Minutes.ToString().PadLeft(2, '0') + ":" +
                time.Seconds.ToString().PadLeft(2, '0') + "." +
                time.Milliseconds.ToString().PadLeft(3, '0');
        }

        private int GetValueCellIndex(DataGridView dataGridView)
        {
            for (int i = 0; i < dataGridView.Columns.Count; i++)
            {
                if (dataGridView.Columns[i].HeaderText == "Value")
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
