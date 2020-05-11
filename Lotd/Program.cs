using Lotd.UI;
using Lotd.FileFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Lotd
{
    class Program
    {
        public static MemTools MemTools { get; private set; }
        public static Manager Manager { get; private set; }

        // This should really be passed around between classes rather than being a application wide variable. To simplify the updating of existing code we are
        // doing it like this for now as otherwise there would need to be lots of changes such as with the class 'Constants', etc.
        public static GameVersion Version { get; set; }

        [STAThread]
        static void Main()
        {
            //TestModifyCardNamedBin(GameVersion.LinkEvolution2);
            //UnlockDLC(GameVersion.LinkEvolution2);
            //Dump(GameVersion.LinkEvolution2);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                NativeScriptCompiler.CompileIfChanged();
            }

            Dictionary<GameVersion, string> installedVersions = new Dictionary<GameVersion, string>();
            foreach (GameVersion version in Enum.GetValues(typeof(GameVersion)))
            {
                if (version == GameVersion.LinkEvolution1)
                {
                    continue;
                }
                string dir = LotdArchive.GetInstallDirectory(version);
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                {
                    installedVersions[version] = dir;
                }
            }

            if (installedVersions.Count == 0)
            {
                MessageBox.Show("Failed to find LOTD install directory. Make sure you have LOTD installed via Steam (and hopefully not pirated!)");
                return;
            }
            else if (installedVersions.Count == 1)
            {
                Version = installedVersions.First().Key;
            }
            else
            {
                if (MessageBox.Show("Target LOTD original version from 2016?", "LOTD", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Version = GameVersion.Lotd;
                }
                else
                {
                    Version = GameVersion.LinkEvolution2;
                }
            }

            Manager = new Manager(Version);
            Manager.Load();

            YdkHelper.LoadIdMap();
            //YdkHelper.GenerateIdMap();

            MemTools = new MemTools(Version);
            MemTools.UseScreenStateTransitions = true;
            MemTools.CustomYdcBattlePacksEnabled = true;
            MemTools.RunProcessWatcher();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DuelStarterForm());

            MemTools.StopProcessWatcher();
        }

        public static void UnlockDLC(GameVersion version)
        {
            LotdArchive archive = new LotdArchive(version);
            archive.WriteAccess = true;
            archive.Load();
            UnlockDLC(archive);
            System.Diagnostics.Debugger.Break();
        }

        public static void UnlockDLC(LotdArchive archive)
        {
            if (!archive.WriteAccess)
            {
                Console.WriteLine("Write access required");
            }

            // It is important that we don't write any new bytes here as we are writing directly
            // to the archive. If any new bytes are added the .dat/.toc will be out of sync and the
            // archive will be corrupted.

            BinaryWriter writer = new BinaryWriter(archive.Reader.BaseStream);

            List<CharData> charDataFiles = archive.LoadFiles<CharData>();
            foreach (CharData charData in charDataFiles)
            {
                foreach (CharData.Item item in charData.Items.Values)
                {
                    item.DlcId = -1;
                }
                
                writer.BaseStream.Position = charData.File.ArchiveOffset;
                charData.Save(writer);
            }

            List<DuelData> duelDataFiles = archive.LoadFiles<DuelData>();
            foreach (DuelData duelData in duelDataFiles)
            {
                foreach (DuelData.Item item in duelData.Items.Values)
                {
                    item.DlcId = -1;
                }
                
                writer.BaseStream.Position = duelData.File.ArchiveOffset;
                duelData.Save(writer);
            }
        }

        /// <summary>
        /// Testing changes to CARD_Named.bin
        /// </summary>
        private static void TestModifyCardNamedBin(GameVersion version)
        {
            LotdArchive archive = new LotdArchive(version);
            archive.WriteAccess = true;
            archive.Load();

            Dictionary<CardNameType, List<short>> cardNameTypes = new Dictionary<CardNameType, List<short>>();

            BinaryWriter writer = new BinaryWriter(archive.Reader.BaseStream, Encoding.Default, true);
            LotdFile file = archive.Root.FindFile("bin/CARD_Named.bin");
            /*foreach (byte b in file.LoadBuffer())
            {
                Console.Write(b.ToString("X2") + " ");
            }*/
            using (BinaryReader reader = new BinaryReader(new MemoryStream(file.LoadBuffer())))
            {
                ushort numArchetypes = reader.ReadUInt16();
                ushort numCards = reader.ReadUInt16();

                long cardsStartOffset = 4 + (numArchetypes * 4);
                long cardsEndOffset = cardsStartOffset + (numCards * 2);
                System.Diagnostics.Debug.Assert(reader.BaseStream.Length == cardsEndOffset);

                for (int i = 0; i < numArchetypes; i++)
                {
                    int offset = reader.ReadInt16();// The offset of the cards for this named group (starts at 0)
                    int count = reader.ReadInt16();// The number of cards for this named group
                    List<short> cardIds;
                    if (!cardNameTypes.TryGetValue((CardNameType)i, out cardIds))
                    {
                        cardNameTypes.Add((CardNameType)i, cardIds = new List<short>());
                    }

                    long tempOffset = reader.BaseStream.Position;
                    reader.BaseStream.Position = cardsStartOffset + (offset * 2);
                    for (int j = 0; j < count; j++)
                    {
                        short cardId = reader.ReadInt16();
                        cardIds.Add(cardId);
                    }
                    reader.BaseStream.Position = tempOffset;
                }
            }

            int totalCards = 0;
            int cardsOffset = 0;
            foreach (KeyValuePair<CardNameType, List<short>> cards in cardNameTypes)
            {
                totalCards += cards.Value.Count;
            }

            writer.BaseStream.Position = file.ArchiveOffset;
            writer.Write((ushort)cardNameTypes.Count);
            writer.Write((ushort)totalCards);// total num cards
            foreach (KeyValuePair<CardNameType, List<short>> cards in cardNameTypes)
            {
                writer.Write((ushort)cardsOffset);// The offset of the cards for this named group (starts at 0)
                writer.Write((ushort)cards.Value.Count);// The number of cards for this named group
                cardsOffset += cards.Value.Count;
            }
            foreach (KeyValuePair<CardNameType, List<short>> cards in cardNameTypes)
            {
                if (cards.Key == CardNameType.UA)
                {
                    //cards.Value.Remove(11637);
                    cards.Value.Add(11641);
                    cards.Value.Sort();
                }

                //bool first = true;
                foreach (short cardId in cards.Value)//.OrderBy(x => x))
                {
                    if ((cards.Key == CardNameType.UA/* && first*/))// || cards.Key > CardNameType.PendDragon)
                    {
                        /*if (cardId == 11637)//(cards.Key == CardNameType.UA && first))
                        {
                            Console.WriteLine(cardId);
                            writer.Write((short)5380);
                        }
                        else*/
                        {
                            writer.Write(cardId);
                        }
                    }
                    else
                    {
                        writer.Write(cardId);
                    }
                    //first = false;
                }
            }
            writer.Close();

            System.Diagnostics.Debugger.Break();
        }

        private static void Dump(GameVersion version)
        {
            LotdArchive archive = new LotdArchive(version);
            archive.Load();
            archive.Dump("Dump");
        }
    }
}
