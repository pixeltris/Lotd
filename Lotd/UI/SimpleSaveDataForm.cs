using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lotd.UI
{
    public partial class SimpleSaveDataForm : Form
    {
        public SimpleSaveDataForm()
        {
            InitializeComponent();
        }

        private void applyButton_Click(object sender, EventArgs e)
        {
            bool saveToMemory = Program.MemTools != null && Program.MemTools.HasProcessHandle;

            GameSaveData saveData = new GameSaveData();

            if (saveToMemory)
            {
                byte[] saveBuffer = Program.MemTools.ReadSaveData();
                if (saveBuffer == null)
                {
                    return;
                }
                if (!saveData.Load(saveBuffer))
                {
                    return;
                }
            }
            else
            {
                if (!saveData.Load())
                {
                    return;
                }
            }

            SetCampaignState(saveData, DuelSeries.YuGiOh,
                campaignYuGiOhAvailable0PercentRadioButton.Checked,
                campaignYuGiOh0PercentRadioButton.Checked,
                campaignYuGiOh100PercentRadioButton.Checked);

            SetCampaignState(saveData, DuelSeries.YuGiOhGX,
                campaignGXAvailable0PercentRadioButton.Checked,
                campaignGX0PercentRadioButton.Checked,
                campaignGX100PercentRadioButton.Checked);

            SetCampaignState(saveData, DuelSeries.YuGiOh5D,
                campaign5DsAvailable0PercentRadioButton.Checked,
                campaign5Ds0PercentRadioButton.Checked,
                campaign5Ds100PercentRadioButton.Checked);

            SetCampaignState(saveData, DuelSeries.YuGiOhZEXAL,
                campaignZexalAvailable0PercentRadioButton.Checked,
                campaignZexal0PercentRadioButton.Checked,
                campaignZexal100PercentRadioButton.Checked);

            SetCampaignState(saveData, DuelSeries.YuGiOhARCV,
                campaignArcVAvailable0PercentRadioButton.Checked,
                campaignArcV0PercentRadioButton.Checked,
                campaignArcV100PercentRadioButton.Checked);

            if (shopPacks0PercentRadioButton.Checked)
            {
                saveData.Misc.UnlockedShopPacks = UnlockedShopPacks.None;
            }
            else if (shopPacks100PercentRadioButton.Checked)
            {
                saveData.Misc.UnlockedShopPacks = UnlockedShopPacks.All;
            }

            if (battlePacks0PercentRadioButton.Checked)
            {
                saveData.Misc.UnlockedBattlePacks = UnlockedBattlePacks.None;
            }
            else if (battlePacks100PercentRadioButton.Checked)
            {
                saveData.Misc.UnlockedBattlePacks = UnlockedBattlePacks.All;
            }

            if (cardsAll0xRadioButton.Checked)
            {
                saveData.SetAllOwnedCardsCount(0, false);
            }
            else if (cardsAll1xRadioButton.Checked)
            {
                saveData.SetAllOwnedCardsCount(1);
            }
            else if (cardsAll2xRadioButton.Checked)
            {
                saveData.SetAllOwnedCardsCount(2);
            }
            else if (cardsAll3xRadioButton.Checked)
            {
                saveData.SetAllOwnedCardsCount(3);
            }

            if (challengesAvailable0PercentRadioButton.Checked)
            {
                for (int i = 0; i < saveData.Misc.Challenges.Length; i++)
                {
                    saveData.Misc.Challenges[i] = DeulistChallengeState.Available;
                }
            }
            else if (challenges0PercentRadioButton.Checked)
            {
                for (int i = 0; i < saveData.Misc.Challenges.Length; i++)
                {
                    saveData.Misc.Challenges[i] = DeulistChallengeState.Locked;
                }
            }
            else if (challenges100PercentRadioButton.Checked)
            {
                for (int i = 0; i < saveData.Misc.Challenges.Length; i++)
                {
                    saveData.Misc.Challenges[i] = DeulistChallengeState.Complete;
                }
            }

            if (deckRecipes0PercentRadioButton.Checked)
            {
                for (int i = 0; i < saveData.Misc.UnlockedRecipes.Length; i++)
                {
                    saveData.Misc.UnlockedRecipes[i] = false;
                }
            }
            else if (deckRecipes100PercentRadioButton.Checked)
            {
                for (int i = 0; i < saveData.Misc.UnlockedRecipes.Length; i++)
                {
                    saveData.Misc.UnlockedRecipes[i] = true;
                }
            }

            if (avatars0PercentRadioButton.Checked)
            {
                for (int i = 0; i < saveData.Misc.UnlockedAvatars.Length; i++)
                {
                    saveData.Misc.UnlockedAvatars[i] = false;
                }
            }
            else if (avatars100PercentRadioButton.Checked)
            {
                for (int i = 0; i < saveData.Misc.UnlockedAvatars.Length; i++)
                {
                    saveData.Misc.UnlockedAvatars[i] = true;
                }
            }

            int duelPoints;
            if (!string.IsNullOrWhiteSpace(duelPointsTextBox.Text) &&
                int.TryParse(duelPointsTextBox.Text, out duelPoints))
            {
                saveData.SetDuelPoints(duelPoints);
            }

            if (unlockButtonsCheckBox.Checked)
            {
                saveData.Misc.UnlockedContent = UnlockedContent.All;
            }

            if (removeDefaultCardsCheckBox.Checked)
            {
                Program.MemTools.HideDefaultStructureDeckCards();
            }

            if (saveToMemory)
            {
                byte[] buffer = saveData.ToArray();
                if (buffer != null && buffer.Length == GameSaveData.FileLength)
                {
                    Program.MemTools.WriteSaveData(buffer);
                }
            }
            else
            {
                saveData.Save();
            }
        }

        private void SetCampaignState(GameSaveData saveData, DuelSeries series, bool p0Available, bool p0, bool p100)
        {
            if (p0Available)
            {
                SetCampaignState(saveData, series, CampaignDuelState.AvailableAttempted);
            }
            else if (p0)
            {
                SetCampaignState(saveData, series, CampaignDuelState.Locked);
            }
            else if (p100)
            {
                SetCampaignState(saveData, series, CampaignDuelState.Complete);
            }
        }

        private void SetCampaignState(GameSaveData saveData, DuelSeries series, CampaignDuelState state)
        {
            CampaignSaveData.Duel[] duels = saveData.Campaign.DuelsBySeries[series];

            // Index 0 is reserved (it should always be "Available" or the series will be broken)
            // - Should we set it to Available here in case something else broke it and the user wants it fixed?

            for (int i = 1; i < duels.Length; i++)
            {
                if (i == 1 && state == CampaignDuelState.Locked)
                {
                    // Set the first item to Available or nothing in the series will be playable
                    duels[i].State = CampaignDuelState.Available;
                    duels[i].ReverseDuelState = state;
                }
                else
                {
                    duels[i].State = state;
                    duels[i].ReverseDuelState = state;
                }
            }
        }
    }
}
