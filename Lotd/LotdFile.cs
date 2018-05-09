using Lotd.FileFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd
{
    public class LotdFile
    {
        private FileData cachedData;

        public long ArchiveOffset { get; set; }
        public long ArchiveLength { get; set; }

        public bool IsArchiveFile
        {
            get { return ArchiveLength > 0; }
        }

        public bool IsFileOnDisk
        {
            get { return !string.IsNullOrEmpty(FilePathOnDisk) && File.Exists(FilePathOnDisk); }
        }

        /// <summary>
        /// The file path on disk (use this when adding new files from disk)
        /// </summary>
        public string FilePathOnDisk { get; set; }

        public LotdArchive Archive { get; set; }

        private LotdDirectory directory;
        public LotdDirectory Directory
        {
            get { return directory; }
            set
            {
                if (directory != value)
                {
                    if (directory != null)
                    {
                        directory.Files.Remove(Name);
                    }

                    directory = value;

                    if (directory != null)
                    {
                        directory.Files.Add(Name, this);
                        Archive = directory.Archive;
                    }
                    else
                    {
                        Archive = null;
                    }
                }
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    if (Directory != null)
                    {
                        Directory.Files.Remove(name);
                    }

                    name = value;

                    if (Directory != null)
                    {
                        Directory.Files.Add(name, this);
                    }
                }
            }
        }

        public string FullName
        {
            get { return Directory == null || Directory.IsRoot ? Name : Path.Combine(Directory.FullName, Name); }
        }

        public string Extension
        {
            get { return Path.GetExtension(Name); }
        }

        public LotdFileType FileType
        {
            get { return GetFileTypeFromExtension(Name, Extension); }
        }

        public void Dump(string outputDir)
        {
            Dump(new DumpSettings(outputDir));
        }

        public void Dump(DumpSettings settings)
        {
            LoadData(false).Dump(settings);
        }

        public FileData GetData()
        {
            return GetData(false);
        }

        public FileData GetData(bool cache)
        {
            if (cachedData != null)
            {
                return cachedData;
            }

            FileData fileData = CreateFileData(FileType);
            fileData.File = this;
            if (cache)
            {
                cachedData = fileData;
            }
            return fileData;
        }

        public T LoadData<T>() where T : FileData
        {
            return LoadData() as T;
        }

        public T LoadData<T>(bool cache) where T : FileData
        {
            return LoadData(cache) as T;
        }

        public FileData LoadData()
        {
            return LoadData(true);
        }

        public FileData LoadData(bool cache)
        {
            if (cachedData != null)
            {
                return cachedData;
            }

            FileData fileData = CreateFileData(FileType);
            fileData.File = this;
            if (!fileData.Load())
            {
                return null;
            }
            if (cache)
            {
                cachedData = fileData;
            }
            return fileData;
        }

        public byte[] LoadBuffer()
        {
            FileData fileData = CreateFileData(FileType);
            fileData.File = this;
            return fileData.LoadBuffer();
        }

        public void UnloadData()
        {
            cachedData.Unload();
            cachedData = null;
        }

        public LotdFile GetLocalizedFile(Language language)
        {
            if (GetLanguageFromFileName(Name) == language)
            {
                return this;
            }

            string fileName = GetFileNameWithLanguage(Name, language);
            if (!string.IsNullOrEmpty(fileName))
            {
                return Archive.Root.FindFile(Path.Combine(Directory.FullName, fileName));
            }

            return null;
        }

        internal static LotdFileType GetFileTypeFromExtension(string name, string extension)
        {            
            if (string.IsNullOrEmpty(extension))
            {
                return LotdFileType.Unknown;
            }

            name = name.ToLower();

            switch (extension.ToLower())
            {
                case ".bin":
                    switch (name)
                    {
                        default:
                            if (name.Contains("battlepack"))
                            {
                                return LotdFileType.BattlePackBin;
                            }
                            else if (name.StartsWith("packdata_"))
                            {
                                return LotdFileType.PackDataBin;
                            }
                            else if (name.StartsWith("howtoplay_"))
                            {
                                return LotdFileType.HowToPlayBin;
                            }
                            else if (name.StartsWith("skudata_"))
                            {
                                return LotdFileType.SkuDataBin;
                            }
                            else if (name.StartsWith("arenadata_"))
                            {
                                return LotdFileType.ArenaDataBin;
                            }
                            else if (name.StartsWith("chardata_"))
                            {
                                return LotdFileType.CharDataBin;
                            }
                            else if (name.StartsWith("deckdata_"))
                            {
                                return LotdFileType.DeckDataBin;
                            }
                            else if (name.StartsWith("dueldata_"))
                            {
                                return LotdFileType.DuelDataBin;
                            }
                            else if (name.StartsWith("packdefdata_"))
                            {
                                return LotdFileType.PackDefDataBin;
                            }
                            else if (name.StartsWith("scriptdata_"))
                            {
                                return LotdFileType.ScriptDataBin;
                            }
                            else if (name.StartsWith("pd_limits"))
                            {
                                return LotdFileType.CardLimits;
                            }
                            else if (name.StartsWith("tagdata"))
                            {
                                return LotdFileType.RelatedCardsBin;
                            }
                            break;
                    }
                    break;
                case ".dat":
                    switch (name)
                    {
                        case "credits.dat": return LotdFileType.CreditsDat;
                    }
                    break;                
                case ".bnd": return LotdFileType.StringBnd;
                case ".dfymoo": return LotdFileType.Dfymoo;
                case ".zib": return LotdFileType.Zib;
            }

            return LotdFileType.Unknown;
        }

        internal static FileData CreateFileData(LotdFileType fileType)
        {
            Type type = GetFileType(fileType);
            if (type == null)
            {
                type = typeof(RawFileData);
            }
            return Activator.CreateInstance(type) as FileData;
        }

        public static Type GetFileType(LotdFileType fileType)
        {
            Type type;
            fileTypeLookup.TryGetValue(fileType, out type);
            return type;
        }

        public static LotdFileType GetFileType(Type type)
        {
            LotdFileType fileType;
            fileTypeLookupReverse.TryGetValue(type, out fileType);
            return fileType;
        }

        public static Language GetLanguageFromFileName(string fileName)
        {
            int lastUnderscore = fileName.LastIndexOf('_');
            int lastDot = fileName.LastIndexOf('.');
            if (lastUnderscore >= 0 && fileName.Length > lastUnderscore + 1)
            {
                if (lastDot == -1 || lastDot == lastUnderscore + 2)
                {
                    switch (char.ToUpper(fileName[lastUnderscore + 1]))
                    {
                        case 'E': return Language.English;
                        case 'F': return Language.French;
                        case 'G': return Language.German;
                        case 'I': return Language.Italian;
                        case 'S': return Language.Spanish;
                    }
                }
            }
            return Language.Unknown;
        }

        public static string GetFileNameWithLanguage(string fileName, Language language)
        {
            Language existingLanguage = GetLanguageFromFileName(fileName);
            if (existingLanguage != Language.Unknown)
            {
                char languageChar = 'E';
                switch (language)
                {
                    case Language.English: languageChar = 'E'; break;
                    case Language.French: languageChar = 'F'; break;
                    case Language.German: languageChar = 'G'; break;
                    case Language.Italian: languageChar = 'I'; break;
                    case Language.Spanish: languageChar = 'S'; break;
                }
                
                StringBuilder result = new StringBuilder(fileName);
                result[fileName.LastIndexOf('_') + 1] = languageChar;
                return result.ToString();
            }
            return null;
        }

        static Dictionary<LotdFileType, Type> fileTypeLookup = new Dictionary<LotdFileType, Type>();
        static Dictionary<Type, LotdFileType> fileTypeLookupReverse = new Dictionary<Type, LotdFileType>();
        static LotdFile()
        {
            AddFileType(LotdFileType.BattlePackBin, typeof(BattlePackData));
            AddFileType(LotdFileType.PackDataBin, typeof(ShopPackData));
            AddFileType(LotdFileType.HowToPlayBin, typeof(HowToPlay));
            AddFileType(LotdFileType.SkuDataBin, typeof(SkuData));
            AddFileType(LotdFileType.ArenaDataBin, typeof(ArenaData));
            AddFileType(LotdFileType.CharDataBin, typeof(CharData));
            AddFileType(LotdFileType.DeckDataBin, typeof(DeckData));
            AddFileType(LotdFileType.DuelDataBin, typeof(DuelData));
            AddFileType(LotdFileType.PackDefDataBin, typeof(PackDefData));
            AddFileType(LotdFileType.ScriptDataBin, typeof(ScriptData));
            AddFileType(LotdFileType.CardLimits, typeof(CardLimits));
            AddFileType(LotdFileType.RelatedCardsBin, typeof(RelatedCardData));
            AddFileType(LotdFileType.CreditsDat, typeof(Credits));
            AddFileType(LotdFileType.StringBnd, typeof(StringBnd));
            AddFileType(LotdFileType.Dfymoo, typeof(Dfymoo));
            AddFileType(LotdFileType.Zib, typeof(ZibData));
        }

        private static void AddFileType(LotdFileType fileType, Type type)
        {
            fileTypeLookup.Add(fileType, type);
            fileTypeLookupReverse.Add(type, fileType);
        }
    }
}
