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

        [STAThread]
        static void Main()
        {
            //UnlockDLC();

            if (System.Diagnostics.Debugger.IsAttached)
            {
                NativeScriptCompiler.CompileIfChanged();
            }

            Manager = new Manager();
            Manager.Load();

            YdkHelper.LoadIdMap();
            //YdkHelper.GenerateIdMap();

            MemTools = new MemTools();
            MemTools.UseScreenStateTransitions = true;
            MemTools.CustomYdcBattlePacksEnabled = true;
            MemTools.RunProcessWatcher();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DuelStarterForm());
        }

        public static void UnlockDLC()
        {
            LotdArchive archive = new LotdArchive();
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
    }
}
