using Lotd.FileFormats;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotd
{
    // Using a "Lotd" prefix on these file names to avoid conflicts with System.IO and other things
    // as these are pretty commonly used names.

    /// <summary>
    /// The root archive loaded from .toc / .dat files.
    /// </summary>
    public class LotdArchive
    {
        public string TocFileName
        {
            get
            {
                switch (Version)
                {
                    default:
                    case GameVersion.Lotd:
                    case GameVersion.LinkEvolution1:
                        return "YGO_DATA.toc";
                    case GameVersion.LinkEvolution2:
                        return "YGO_2020.toc";
                }
            }
        }
        public string DatFileName
        {
            get
            {
                switch (Version)
                {
                    default:
                    case GameVersion.Lotd:
                    case GameVersion.LinkEvolution1:
                        return "YGO_DATA.dat";
                    case GameVersion.LinkEvolution2:
                        return "YGO_2020.dat";
                }
            }
        }

        public string InstallDirectory { get; private set; }
        
        public LotdDirectory Root { get; private set; }
        public BinaryReader Reader { get; private set; }

        public bool WriteAccess { get; set; }

        public GameVersion Version { get; private set; }

        public LotdArchive()
        {
            // Order by release date (newest first)
            GameVersion[] versions = { GameVersion.LinkEvolution2, GameVersion.Lotd };

            foreach (GameVersion version in versions)
            {
                string dir = GetInstallDirectory(version);
                if (!string.IsNullOrEmpty(dir))
                {
                    Version = version;
                    InstallDirectory = dir;
                }
            }

            if (string.IsNullOrEmpty(InstallDirectory))
            {
                throw new Exception("Failed to find LOTD data files");
            }
        }

        public LotdArchive(GameVersion version)
        {
            Version = version;
            InstallDirectory = GetInstallDirectory(version);
        }

        public LotdArchive(GameVersion version, string installDirectory)
        {
            Version = version;
            InstallDirectory = installDirectory;
        }

        public void Load()
        {
            if (string.IsNullOrEmpty(InstallDirectory) || !Directory.Exists(InstallDirectory))
            {
                throw new Exception("Couldn't find the install directory for Legacy of the Duelist '" + InstallDirectory + "'");
            }

            string tocPath = Path.Combine(InstallDirectory, TocFileName);
            string datPath = Path.Combine(InstallDirectory, DatFileName);

            if (!File.Exists(tocPath) || !File.Exists(datPath))
            {
                throw new Exception("Failed to find data files");
            }

            if (Reader != null)
            {
                Reader.Close();
                Reader = null;
            }

            Root = new LotdDirectory();
            Root.Archive = this;
            Root.IsRoot = true;

            List<string> filePaths = new List<string>();

            try
            {
                long offset = 0;

                string[] lines = File.ReadAllLines(tocPath);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    if (!line.StartsWith("UT"))
                    {
                        int offsetStart = -1;
                        for (int charIndex = 0; charIndex < line.Length; charIndex++)
                        {
                            if (line[charIndex] != ' ')
                            {
                                offsetStart = charIndex;
                                break;
                            }
                        }

                        int offsetEnd = offsetStart == -1 ? -1 : line.IndexOf(' ', offsetStart);
                        int unknownStart = offsetEnd == -1 ? -1 : offsetEnd + 1;
                        int unknownEnd = unknownStart == -1 ? -1 : line.IndexOf(' ', unknownStart + 1);

                        bool validLine = unknownEnd >= 0;

                        if (validLine)
                        {
                            string lengthStr = line.Substring(offsetStart, offsetEnd - offsetStart);
                            string filePathLengthStr = line.Substring(unknownStart, unknownEnd - unknownStart);
                            string filePath = line.Substring(unknownEnd + 1);

                            long length;
                            int filePathLength;
                            if (long.TryParse(lengthStr, NumberStyles.HexNumber, null, out length) &&
                                int.TryParse(filePathLengthStr, NumberStyles.HexNumber, null, out filePathLength) &&
                                filePathLength == filePath.Length)
                            {
                                Root.AddFile(filePath, offset, length);

                                offset += length;

                                // Add the offset for the data alignment
                                const int align = 4;
                                if (length % align != 0)
                                {
                                    offset += align - (length % align);
                                }

                                filePaths.Add(filePath);
                            }
                            else
                            {
                                validLine = false;
                            }
                        }

                        if (!validLine)
                        {
                            throw new Exception("Failed to parse line in toc file " + line);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error when reading .toc file: " + e);
            }

            try
            {
                if (WriteAccess)
                {
                    Reader = new BinaryReader(File.Open(datPath, FileMode.Open, FileAccess.ReadWrite));
                }
                else
                {
                    Reader = new BinaryReader(File.OpenRead(datPath));
                }                
            }
            catch (Exception e)
            {
                    throw new Exception("Error when opening .dat file: " + e);
            }

            // Validate all file paths
            foreach (string filePath in filePaths)
            {
                LotdFile file = Root.FindFile(filePath);
                if (file == null)
                {
                    throw new Exception("Archive loader is broken. File path not found in archive structure: '" + filePath + "'");
                }
            }
        }

        public void Save()
        {
            Save(InstallDirectory);
        }

        public void Save(bool createBackup)
        {
            Save(InstallDirectory, createBackup);
        }

        public void Save(string outputDir)
        {
            Save(outputDir, true);
        }

        public void Save(string outputDir, bool createBackup)
        {
            if (string.IsNullOrEmpty(outputDir) || !Directory.Exists(outputDir))
            {
                throw new Exception("Invalid directory. Make sure the directory exists. '" + outputDir + "'");
            }

            throw new NotImplementedException();
        }

        public void Dump(string outputDir)
        {
            Dump(new DumpSettings(outputDir));
        }

        public void Dump(DumpSettings settings)
        {
            Root.Dump(settings);
        }

        public List<T> LoadFiles<T>() where T : FileData
        {
            LotdFileType targetFileType = LotdFile.GetFileType(typeof(T));

            List<T> result = new List<T>();

            List<LotdFile> files = Root.GetAllFiles();
            foreach (LotdFile file in files)
            {
                if (file.FileType == LotdFileType.Zib)
                {
                    ZibData zibData = file.LoadData<ZibData>();
                    foreach (ZibFile zibFile in zibData.Files.Values)
                    {
                        if (zibFile.FileType == targetFileType)
                        {
                            T data = zibFile.LoadData<T>();
                            if (data != null)
                            {
                                result.Add(data);
                            }
                        }
                    }
                }
                else if (file.FileType == targetFileType)
                {
                    T data = file.LoadData<T>();
                    if (data != null)
                    {
                        result.Add(data);
                    }
                }
            }

            return result;
        }

        public Dictionary<Language, byte[]> LoadLocalizedBuffer(string search, bool startsWithElseContains)
        {
            Dictionary<Language, byte[]> result = new Dictionary<Language, byte[]>();

            search = search.ToLower();

            List<LotdFile> files = Root.GetAllFiles();
            foreach (LotdFile file in files)
            {
                if (file.FileType == LotdFileType.Zib)
                {
                    ZibData zibData = file.LoadData<ZibData>();
                    foreach (ZibFile zibFile in zibData.Files.Values)
                    {
                        Language language = LotdFile.GetLanguageFromFileName(zibFile.FileName);
                        if (language != Language.Unknown)
                        {
                            if ((startsWithElseContains && zibFile.FileName.ToLower().StartsWith(search)) ||
                                (!startsWithElseContains && zibFile.FileName.ToLower().Contains(search)))
                            {
                                result.Add(language, zibFile.LoadBuffer());
                            }
                        }
                    }
                }
                else
                {
                    Language language = LotdFile.GetLanguageFromFileName(file.Name);
                    if (language != Language.Unknown)
                    {
                        if ((startsWithElseContains && file.Name.ToLower().StartsWith(search)) ||
                            (!startsWithElseContains && file.Name.ToLower().Contains(search)))
                        {
                            result.Add(language, file.LoadBuffer());
                        }
                    }
                }
            }

            return result;
        }

        public T LoadLocalizedFile<T>() where T : FileData, new()
        {
            LotdFileType targetFileType = LotdFile.GetFileType(typeof(T));

            T result = new T();
            if (!result.IsLocalized)
            {
                throw new InvalidOperationException("Attempted to load a file with localization which has none");
            }

            List<LotdFile> files = Root.GetAllFiles();
            foreach (LotdFile file in files)
            {
                if (file.FileType == LotdFileType.Zib)
                {
                    ZibData zibData = file.LoadData<ZibData>();
                    foreach (ZibFile zibFile in zibData.Files.Values)
                    {
                        if (zibFile.FileType == targetFileType)
                        {
                            result.File = null;
                            result.ZibFile = zibFile;
                            result.Load();
                        }
                    }
                }
                else if (file.FileType == targetFileType)
                {
                    result.File = file;
                    result.ZibFile = null;
                    result.Load();
                }
            }

            return result;
        }

        internal void RunTests()
        {
            TestFileType<Credits>();
            TestFileType<StringBnd>();
            TestFileType<BattlePackData>();
            TestFileType<ShopPackData>();
            TestFileType<HowToPlay>();
            TestFileType<SkuData>();
            TestFileType<ArenaData>();
            TestFileType<CharData>();
            TestFileType<DeckData>();
            TestFileType<DuelData>();
            TestFileType<PackDefData>();
            TestFileType<ScriptData>();
            TestFileType<CardLimits>();
            TestFileType<RelatedCardData>();
            TestFileType<Dfymoo>();
        }

        private List<T> TestFileType<T>() where T : FileData
        {
            List<T> result = LoadFiles<T>();
            foreach (T data in result)
            {
                ValidateData(data);
            }
            return result;
        }

        private void ValidateData(FileData file)
        {
            // Add some padding as in real data we wont be starting at offset 0
            byte[] padding = Enumerable.Repeat((byte)0xFF, 100).ToArray();

            // Validate our Save function
            byte[] buffer = file.ZibFile != null ? file.ZibFile.LoadBuffer() : file.File.LoadBuffer();

            byte[] paddedBuffer = new byte[buffer.Length + padding.Length];
            Buffer.BlockCopy(padding, 0, paddedBuffer, 0, padding.Length);
            Buffer.BlockCopy(buffer, 0, paddedBuffer, padding.Length, buffer.Length);

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(padding);

                file.Save(bw);
                byte[] buffer2 = ms.ToArray();
                if (file.FileType == LotdFileType.Dfymoo)
                {
                    System.Diagnostics.Debug.Assert(
                        Encoding.ASCII.GetString(paddedBuffer).TrimEnd('\r', '\n') ==
                        Encoding.ASCII.GetString(buffer2).TrimEnd('\r', '\n'));
                }
                else
                {
                    for (int i = 0; i < paddedBuffer.Length; i++)
                    {
                        System.Diagnostics.Debug.Assert(paddedBuffer[i] == buffer2[i]);
                    }
                    System.Diagnostics.Debug.Assert(paddedBuffer.SequenceEqual(buffer2));
                }
            }
        }

        public static string GetInstallDirectory(GameVersion version)
        {
            string installDir = null;

            int appId = 0;
            string gameName = "";
            switch (version)
            {
                case GameVersion.Lotd:
                    appId = 480650;
                    gameName = "Yu-Gi-Oh! Legacy of the Duelist";
                    break;
                case GameVersion.LinkEvolution1:
                case GameVersion.LinkEvolution2:
                    appId = 1150640;
                    gameName = "Yu-Gi-Oh! Legacy of the Duelist Link Evolution";
                    break;
            }

            if (string.IsNullOrEmpty(gameName))
            {
                return null;
            }

            try
            {
                using (var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (var key = root.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App " + appId))
                    {
                        if (key != null)
                        {
                            installDir = key.GetValue("InstallLocation").ToString();
                        }
                    }
                }
            }
            catch
            {
            }

            if (string.IsNullOrEmpty(installDir) || !Directory.Exists(installDir))
            {
                try
                {
                    using (var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                    {
                        using (var key = root.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                        {
                            if (key != null)
                            {
                                string steamDir = key.GetValue("InstallPath").ToString();
                                installDir = Path.Combine(steamDir, "steamapps", "common", gameName);
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            return installDir;
        }
    }

    public class DumpSettings
    {
        /// <summary>
        /// The output directory for this dump
        /// </summary>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Dump all files including files in their own containers (zib)
        /// </summary>
        public bool Deep { get; set; }

        /// <summary>
        /// Convert binary files into a human readable format
        /// </summary>
        public bool HumanReadable { get; set; }

        public DumpSettings(string outputDirectory)
        {
            OutputDirectory = outputDirectory;

            if (!string.IsNullOrEmpty(OutputDirectory) && !Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }

            Deep = true;
        }
    }
}
