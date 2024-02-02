using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd.FileFormats
{
    public abstract class FileData
    {
        public LotdFile File { get; set; }
        public ZibFile ZibFile { get; set; }

        public LotdFileType FileType
        {
            get { return File != null ? File.FileType : ZibFile != null ? ZibFile.FileType : LotdFileType.Unknown; }
        }

        public virtual bool IsLocalized
        {
            get { return false; }
        }

        public byte[] LoadBuffer()
        {
            if (File.IsFileOnDisk)
            {
                return LoadBuffer(File.FilePathOnDisk);
            }
            else if (File.IsArchiveFile)
            {
                return LoadBuffer(File.Archive.Reader);
            }
            else
            {
                return null;
            }
        }

        public bool Load()
        {
            if (IsLocalized)
            {
                return Load(GetLanguage());
            }

            if (ZibFile != null)
            {
                if (ZibFile.IsFileOnDisk)
                {
                    if (!System.IO.File.Exists(ZibFile.FilePathOnDisk))
                    {
                        return false;
                    }
                    Load(ZibFile.FilePathOnDisk);
                    return true;
                }
                else if (ZibFile.Owner != null && ZibFile.Owner.File != null && ZibFile.Offset > 0 && ZibFile.Length > 0)
                {
                    ZibFile.Owner.File.Archive.Reader.BaseStream.Position = ZibFile.Owner.File.ArchiveOffset + ZibFile.Offset;
                    Load(ZibFile.Owner.File.Archive.Reader, ZibFile.Length);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (File != null)
            {
                if (File.IsFileOnDisk)
                {
                    if (!System.IO.File.Exists(File.FilePathOnDisk))
                    {
                        return false;
                    }
                    Load(File.FilePathOnDisk);
                    return true;
                }
                else if (File.IsArchiveFile)
                {
                    if (File.CanLoadArchive)
                    {
                        File.Archive.Reader.BaseStream.Position = File.ArchiveOffset;
                        Load(File.Archive.Reader, File.ArchiveLength);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool Load(Language language)
        {
            if (ZibFile != null)
            {
                if (ZibFile.IsFileOnDisk)
                {
                    if (!System.IO.File.Exists(ZibFile.FilePathOnDisk))
                    {
                        return false;
                    }
                    Load(ZibFile.FilePathOnDisk, language);
                    return true;
                }
                else if (ZibFile.Owner != null && ZibFile.Owner.File != null && ZibFile.Offset > 0 && ZibFile.Length > 0)
                {
                    ZibFile.Owner.File.Archive.Reader.BaseStream.Position = ZibFile.Owner.File.ArchiveOffset + ZibFile.Offset;
                    Load(ZibFile.Owner.File.Archive.Reader, ZibFile.Length, language);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (File != null)
            {
                if (File.IsFileOnDisk)
                {
                    if (!System.IO.File.Exists(File.FilePathOnDisk))
                    {
                        return false;
                    }
                    Load(File.FilePathOnDisk, language);
                    return true;
                }
                else if (File.IsArchiveFile)
                {
                    if (File.CanLoadArchive)
                    {
                        File.Archive.Reader.BaseStream.Position = File.ArchiveOffset;
                        Load(File.Archive.Reader, File.ArchiveLength);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public byte[] LoadBuffer(BinaryReader reader)
        {
            File.Archive.Reader.BaseStream.Position = File.ArchiveOffset;
            return reader.ReadBytes((int)File.ArchiveLength);
        }

        public byte[] LoadBuffer(string path)
        {
            if (System.IO.File.Exists(path))
            {
                return System.IO.File.ReadAllBytes(path);
            }
            return null;
        }

        public virtual void Load(BinaryReader reader, long length)
        {
            if (IsLocalized)
            {
                Load(reader, length, GetLanguage());
            }
        }

        public virtual void Load(BinaryReader reader, long length, Language language)
        {
            throw new NotImplementedException("Localized file doesn't implement Load function");
        }

        public virtual void Save(BinaryWriter writer)
        {
            if (IsLocalized)
            {
                Save(writer, GetLanguage());
            }
        }

        public virtual void Save(BinaryWriter writer, Language language)
        {
            throw new NotImplementedException("Localized file doesn't implement Save function");
        }

        public void Load(string path)
        {
            using (BinaryReader reader = new BinaryReader(System.IO.File.OpenRead(path)))
            {
                Load(reader, reader.BaseStream.Length);
            }
        }

        public void Load(string path, Language language)
        {
            using (BinaryReader reader = new BinaryReader(System.IO.File.OpenRead(path)))
            {
                Load(reader, reader.BaseStream.Length, language);
            }
        }

        public void Save(string path)
        {
            using (BinaryWriter writer = new BinaryWriter(System.IO.File.Create(path)))
            {
                Save(writer);
            }
        }

        public void Save(string path, Language language)
        {
            using (BinaryWriter writer = new BinaryWriter(System.IO.File.Create(path)))
            {
                Save(writer, language);
            }
        }

        public virtual void Clear()
        {
        }

        private Language GetLanguage()
        {
            if (File != null)
            {
                return LotdFile.GetLanguageFromFileName(File.Name);
            }
            else if (ZibFile != null)
            {
                return LotdFile.GetLanguageFromFileName(ZibFile.FileName);
            }
            return Language.Unknown;
        }

        public virtual void Dump(string outputDir)
        {
            Dump(new DumpSettings(outputDir));
        }

        public virtual void Dump(DumpSettings settings)
        {
            ShallowDump(settings);
        }

        private void ShallowDump(DumpSettings settings)
        {
            if (File != null)
            {
                byte[] buffer = LoadBuffer();
                if (buffer != null)
                {
                    string outputDir = settings.OutputDirectory;
                    outputDir = Path.Combine(outputDir == null ? string.Empty : outputDir, File.Directory.FullName);
                    if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }
                    System.IO.File.WriteAllBytes(Path.Combine(outputDir, File.Name), buffer);
                }
            }
        }

        public virtual void Unload()
        {
        }

        protected int GetStringSize(string str, Encoding encoding)
        {
            return encoding.GetByteCount((str == null ? string.Empty : str) + '\0');
        }

        public LotdFile GetLocalizedFile(Language language)
        {
            return File == null ? null : File.GetLocalizedFile(language);
        }

        public ZibFile GetLocalizedZibFile(Language language)
        {
            return ZibFile == null ? null : ZibFile.GetLocalizedFile(language);
        }
    }
}
