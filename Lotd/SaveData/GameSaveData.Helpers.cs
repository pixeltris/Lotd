using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd
{
    // Putting some helper functions here to keep the main file clean and relevant to just saving / loading
    public partial class GameSaveData
    {
        public void SetDuelPoints(int duelPoints)
        {
            Misc.DuelPoints = duelPoints;
        }

        /// <summary>
        /// Unlocks all content under "padlocks" which happens early game when you haven't complepted any content
        /// </summary>
        public void UnlockPadlockedContent()
        {
            Misc.UnlockedContent = UnlockedContent.All;
            Misc.UnlockedShopPacks = UnlockedShopPacks.All;
            Misc.UnlockedBattlePacks = UnlockedBattlePacks.All;
        }

        public void UnlockAllRecipes()
        {
            for (int i = 0; i < Misc.UnlockedRecipes.Length; i++)
            {
                Misc.UnlockedRecipes[i] = true;
            }
        }

        public void UnlockAllAvatars()
        {
            for (int i = 0; i < Misc.UnlockedAvatars.Length; i++)
            {
                Misc.UnlockedAvatars[i] = true;
            }
        }

        public void SetAllChallenges(DeulistChallengeState state)
        {
            for (int i = 0; i < Misc.Challenges.Length; i++)
            {
                Misc.Challenges[i] = state;
            }
        }

        public void SetAllCampaignDuels(CampaignDuelState state)
        {
            SetAllCampaignDuels(state, CampaignDuelState.Locked, false);
        }

        public void SetAllCampaignDuels(CampaignDuelState state, CampaignDuelState reverseState)
        {
            SetAllCampaignDuels(state, reverseState, true);
        }

        private void SetAllCampaignDuels(CampaignDuelState state, CampaignDuelState reverseState, bool setReverseState)
        {
            for (int i = 0; i < CampaignSaveData.DuelsPerSeries; i++)
            {
                // The first item MUST be "Available" or the series buttons aren't clickable
                CampaignDuelState tempState = i == 0 ? CampaignDuelState.Available : state;

                Campaign.DuelsBySeries[DuelSeries.YuGiOh][i].State = tempState;
                Campaign.DuelsBySeries[DuelSeries.YuGiOhGX][i].State = tempState;
                Campaign.DuelsBySeries[DuelSeries.YuGiOh5D][i].State = tempState;
                Campaign.DuelsBySeries[DuelSeries.YuGiOhZEXAL][i].State = tempState;
                Campaign.DuelsBySeries[DuelSeries.YuGiOhARCV][i].State = tempState;
                if (setReverseState)
                {
                    Campaign.DuelsBySeries[DuelSeries.YuGiOh][i].ReverseDuelState = reverseState;
                    Campaign.DuelsBySeries[DuelSeries.YuGiOhGX][i].ReverseDuelState = reverseState;
                    Campaign.DuelsBySeries[DuelSeries.YuGiOh5D][i].ReverseDuelState = reverseState;
                    Campaign.DuelsBySeries[DuelSeries.YuGiOhZEXAL][i].ReverseDuelState = reverseState;
                    Campaign.DuelsBySeries[DuelSeries.YuGiOhARCV][i].ReverseDuelState = reverseState;
                }
            }
        }

        public void UnlockAllCards()
        {
            SetAllOwnedCardsCount(3);
        }

        public void SetAllOwnedCardsCount(byte cardCount)
        {
            SetAllOwnedCardsCount(cardCount, true);
        }

        public void SetAllOwnedCardsCount(byte cardCount, bool seen)
        {
            for (int i = 0; i < CardList.Cards.Length; i++)
            {
                // If a card is set to "seen" it wont have the "NEW" icon on the card
                CardList.Cards[i].Seen = seen;
                CardList.Cards[i].Count = cardCount;
            }
        }

        private void SaveSignature(byte[] buffer)
        {
            uint saveCount = BitConverter.ToUInt32(buffer, 16) + 1;
            byte[] saveCountBuf = BitConverter.GetBytes(saveCount);
            Buffer.BlockCopy(saveCountBuf, 0, buffer, 16, 4);

            uint signature = GetSignature(buffer);
            byte[] signatureBuf = BitConverter.GetBytes(signature);
            Buffer.BlockCopy(signatureBuf, 0, buffer, 12, 4);
        }

        private uint GetSignature(byte[] buffer)
        {
            for (int i = 0; i < 4; i++)
            {
                buffer[12 + i] = 0;
            }

            ulong result = 0xFFFFFFFF;
            for (int i = 0; i < buffer.Length; i++)
            {
                result = ((uint)result >> 8) ^ xorTable[(byte)result ^ buffer[i]];
            }
            return (uint)result;
        }

        public void FixGameSaveSignatureOnDisk()
        {
            FixGameSaveSignatureOnDisk(GetSaveFilePath());
        }

        public void FixGameSaveSignatureOnDisk(string path)
        {
            if (File.Exists(path))
            {
                byte[] buffer = File.ReadAllBytes(path);
                SaveSignature(buffer);
                File.WriteAllBytes(path, buffer);
            }
        }

        internal void CopyChunk(int chunkIndex, string fromPath, string toPath)
        {
            if (!File.Exists(fromPath) || !File.Exists(toPath))
            {
                Console.WriteLine("Bad file path for CopyChunk");
                return;
            }

            int chunkStart, chunkEnd;
            if (GetChunkRange(chunkIndex, out chunkStart, out chunkEnd))
            {
                byte[] fromFileBuffer = File.ReadAllBytes(fromPath);
                byte[] toFileBuffer = File.ReadAllBytes(toPath);

                Buffer.BlockCopy(fromFileBuffer, chunkStart, toFileBuffer, chunkStart, chunkEnd - chunkStart);

                SaveSignature(toFileBuffer);
                File.WriteAllBytes(toPath, toFileBuffer);
            }
            else
            {
                Console.WriteLine("Unknown chunk index " + chunkIndex);
            }
        }

        private int GetChunkSize(int chunkIndex)
        {
            int chunkStart, chunkEnd;
            if (GetChunkRange(chunkIndex, out chunkStart, out chunkEnd))
            {
                return chunkEnd - chunkStart;
            }
            return -1;
        }

        private bool GetChunkRange(int chunkIndex, out int chunkStart, out int chunkEnd)
        {
            int[] offsets =
            {
                UnkOffset1, StatsOffset, BattlePacksOffset, MiscDataOffset, CampaignDataOffset,
                DecksOffset, CardListOffset, FileLength
            };

            chunkStart = -1;
            chunkEnd = -1;

            if (chunkIndex >= 0 && chunkIndex <= 6)
            {
                chunkStart = offsets[chunkIndex];
                chunkEnd = offsets[chunkIndex + 1];
                return true;
            }

            return false;
        }

        internal void Diff(string newPath, string oldPath)
        {
            byte[] newBuffer = File.ReadAllBytes(newPath);
            byte[] oldBuffer = File.ReadAllBytes(oldPath);

            Debug.Assert(newBuffer.Length == oldBuffer.Length);
            Debug.WriteLine("offset | new | old");

            for (int i = 16; i < newBuffer.Length; i++)
            {
                if (newBuffer[i] != oldBuffer[i])
                {
                    Debug.WriteLine(i + " | " + newBuffer[i].ToString("X2") + " | " + oldBuffer[i].ToString("X2"));
                }
            }
        }

        public static string GetSaveFilePath()
        {
            string installDir = LotdArchive.GetInstallDirectory();
            if (!string.IsNullOrEmpty(installDir))
            {
                try
                {
                    int steamAppId = 0;

                    string appIdFile = Path.Combine(installDir, "steam_appid.txt");
                    if (File.Exists(appIdFile))
                    {
                        string[] lines = File.ReadAllLines(appIdFile);
                        if (lines.Length > 0)
                        {
                            int.TryParse(lines[0], out steamAppId);
                        }
                    }

                    if (steamAppId > 0)
                    {
                        string userdataDir = Path.Combine(installDir, "../../../userdata/");
                        if (Directory.Exists(userdataDir))
                        {
                            string[] dirs = Directory.GetDirectories(userdataDir);
                            for (int i = 0; i < dirs.Length; i++)
                            {
                                string dirName = new DirectoryInfo(dirs[i]).Name;

                                long userid;
                                if (long.TryParse(dirName, out userid))
                                {
                                    string saveDataDir = Path.Combine(dirs[i], string.Empty + steamAppId, "remote");
                                    if (Directory.Exists(saveDataDir))
                                    {
                                        string saveDataFile = Path.Combine(saveDataDir, "savegame.dat");
                                        if (File.Exists(saveDataFile))
                                        {
                                            return Path.GetFullPath(saveDataFile);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            return null;
        }
    }
}
