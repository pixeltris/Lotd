using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd
{
    public class LotdDirectory
    {
        public LotdArchive Archive { get; set; }

        // Root directory is reserved by the archive. It isn't an actual directory and has no name.
        public bool IsRoot { get; set; }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    if (Parent != null)
                    {
                        Parent.Directories.Remove(name);
                    }

                    name = value;

                    if (Parent != null)
                    {
                        Parent.Directories.Add(name, this);
                    }
                }
            }
        }

        public string FullName
        {
            get
            {
                string name = Name == null ? string.Empty : Name;
                string parentName = null;
                if (Parent != null && !Parent.IsRoot)
                {
                    parentName = Parent.FullName;
                }
                return parentName == null ? name : Path.Combine(parentName, name);
            }
        }

        private LotdDirectory parent;
        public LotdDirectory Parent
        {
            get { return parent; }
            set
            {
                if (parent != value)
                {
                    if (parent != null)
                    {
                        parent.Directories.Remove(Name);
                    }

                    parent = value;

                    if (parent != null)
                    {
                        parent.Directories.Add(Name, this);
                        Archive = parent.Archive;
                    }
                    else
                    {
                        Archive = null;
                    }
                }
            }
        }

        public Dictionary<string, LotdFile> Files { get; private set; }
        public Dictionary<string, LotdDirectory> Directories { get; private set; }

        public LotdDirectory()
        {
            // Paths are likely case insensitive, make sure our dictionaries are too.
            Files = new Dictionary<string, LotdFile>(StringComparer.OrdinalIgnoreCase);
            Directories = new Dictionary<string, LotdDirectory>(StringComparer.OrdinalIgnoreCase);
        }

        public void Dump(string outputDir)
        {
            Dump(new DumpSettings(outputDir));
        }

        public void Dump(DumpSettings settings)
        {
            string outputDir = settings.OutputDirectory;
            string dir = Path.Combine(outputDir == null ? string.Empty : outputDir, FullName);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            foreach (KeyValuePair<string, LotdDirectory> directory in Directories)
            {
                directory.Value.Dump(settings);
            }

            foreach (KeyValuePair<string, LotdFile> file in Files)
            {
                file.Value.Dump(settings);
            }
        }

        public void CreateDirectory(string path)
        {
            LotdDirectory directory = ResolveDirectory(path, false, true);
            if (directory == null)
            {
                throw new Exception("Invalid path '" + path + "'");
            }
        }

        public bool DirectoryExists(string path)
        {
            return FindDirectory(path, false) != null;
        }

        public LotdDirectory FindDirectory(string path)
        {
            return FindDirectory(path, false);
        }

        public LotdDirectory FindDirectory(string path, bool isFilePath)
        {
            return ResolveDirectory(path, false, false);
        }

        public LotdDirectory ResolveDirectory(string path, bool isFilePath, bool create)
        {
            return ResolveDirectory(SplitPath(path), isFilePath, create);
        }

        private LotdDirectory ResolveDirectory(string[] path, bool isFilePath, bool create)
        {
            LotdDirectory directory = this;
            
            int pathCount = path.Length + (isFilePath ? -1 : 0);
            for (int i = 0; i < pathCount; i++)
            {
                if (path[i] == "..")
                {
                    directory = directory.Parent;
                    if (directory == null)
                    {
                        return null;
                    }
                }
                else
                {
                    LotdDirectory subDir;
                    if (directory.Directories.TryGetValue(path[i], out subDir))
                    {
                        directory = subDir;
                    }
                    else
                    {
                        subDir = new LotdDirectory();
                        subDir.Name = path[i];
                        subDir.Parent = directory;
                        directory = subDir;
                    }
                }
            }

            return directory;
        }        
        
        public bool FileExists(string path)
        {
            return FindFile(path) != null;
        }

        public LotdFile FindFile(string path)
        {
            string[] splitted = SplitPath(path);
            if (splitted.Length > 0)
            {
                LotdDirectory directory = ResolveDirectory(splitted, true, false);
                LotdFile file;
                if (directory != null && directory.Files.TryGetValue(splitted[splitted.Length - 1], out file))
                {
                    return file;
                }
            }
            return null;
        }

        public List<LotdFile> GetAllFiles()
        {
            List<LotdFile> files = new List<LotdFile>();
            GetAllFiles(files);
            return files;
        }

        private void GetAllFiles(List<LotdFile> files)
        {
            foreach (LotdFile file in Files.Values)
            {
                files.Add(file);
            }
            foreach (LotdDirectory directory in Directories.Values)
            {
                directory.GetAllFiles(files);
            }
        }

        /// <summary>
        /// Adds all files on disk within a given directory
        /// </summary>
        public LotdFile[] AddFilesOnDisk(string directory, string rootDir, bool recursive)
        {
            List<LotdFile> files = new List<LotdFile>();
            if (Directory.Exists(directory))
            {
                SearchOption searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                foreach (string filePath in Directory.GetFiles(directory, "*.*", searchOption))
                {
                    LotdFile file = AddFileOnDisk(filePath, rootDir);
                    if (file != null)
                    {
                        files.Add(file);
                    }
                }
            }
            return files.ToArray();
        }

        /// <summary>
        /// Adds a file from a file path on disk
        /// </summary>
        public LotdFile AddFileOnDisk(string filePath, string rootDir)
        {
            if (File.Exists(filePath))
            {
                string relativePath = GetRelativeFilePathOnDisk(filePath, rootDir);

                LotdFile existingFile = FindFile(relativePath);
                if (existingFile != null)
                {
                    if (!existingFile.IsFileOnDisk)
                    {
                        // Already exists as an archive file or a placeholder
                        // Set the file path (this will favor loading from the file rather than the archive)
                        existingFile.FilePathOnDisk = filePath;
                    }
                    return existingFile;
                }

                string[] splitted = SplitPath(relativePath);
                if (splitted.Length > 0)
                {
                    LotdDirectory directory = ResolveDirectory(splitted, true, true);
                    if (directory != null)
                    {
                        LotdFile file = new LotdFile();
                        file.Name = splitted[splitted.Length - 1];
                        file.Directory = directory;
                        file.FilePathOnDisk = filePath;
                        return file;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the relative path of a file on disk relative to the given root directory
        /// </summary>
        public string GetRelativeFilePathOnDisk(string filePath, string rootDir)
        {
            return GetRelativePathOnDisk(filePath, rootDir, true);
        }

        /// <summary>
        /// Returns the relative on-disk directory path relative to the given root directory
        /// </summary>
        public string GetRelativeDirectoryOnDisk(string directory, string rootDir)
        {
            return GetRelativePathOnDisk(directory, rootDir, false);
        }

        public string GetRelativePathOnDisk(string path, string rootDir, bool isFile)
        {
            if ((isFile && !File.Exists(path)) || (!isFile && !Directory.Exists(path)))
            {
                return null;
            }

            path = Path.GetFullPath(path);
            rootDir = Path.GetFullPath(rootDir);

            string dir = isFile ? Path.GetDirectoryName(path) : path;
            if (!IsSameOrSubDirectory(rootDir, dir))
            {
                throw new Exception("filePath must be in a sub directory of rootDir");
            }

            Uri pathUri = new Uri(path);
            Uri referenceUri = new Uri(rootDir);
            string relativePath = referenceUri.MakeRelativeUri(pathUri).ToString();

            // TODO: Find a better way to do this. We don't want the starting part of the root directory
            int firstPathIndex = relativePath.IndexOfAny(new char[] { Path.PathSeparator, Path.AltDirectorySeparatorChar });
            if (firstPathIndex >= 0)
            {
                relativePath = relativePath.Substring(firstPathIndex + 1);
            }

            return relativePath;
        }

        public LotdFile AddFile(string path, long offset, long length)
        {
            string[] splitted = SplitPath(path);
            if (splitted.Length > 0)
            {
                LotdDirectory directory = ResolveDirectory(splitted, true, true);
                if (directory != null)
                {
                    LotdFile file = new LotdFile();
                    file.Name = splitted[splitted.Length - 1];
                    file.Directory = directory;
                    file.ArchiveOffset = offset;
                    file.ArchiveLength = length;
                    return file;
                }
            }
            return null;
        }

        public void RemoveFile(string path)
        {
            LotdFile file = FindFile(path);
            if (file != null)
            {
                file.Directory = null;
            }
        }

        public void RemoveFile(LotdFile file)
        {
            if (file.Directory == this)
            {
                file.Directory = null;
            }
        }

        private string[] SplitPath(string path)
        {
            return path.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries);
        }

        private bool IsSameOrSubDirectory(string basePath, string path)
        {
            string subDirectory;
            return IsSameOrSubDirectory(basePath, path, out subDirectory);
        }

        private bool IsSameOrSubDirectory(string basePath, string path, out string subDirectory)
        {
            DirectoryInfo di = new DirectoryInfo(Path.GetFullPath(path).TrimEnd('\\', '/'));
            DirectoryInfo diBase = new DirectoryInfo(Path.GetFullPath(basePath).TrimEnd('\\', '/'));

            subDirectory = null;
            while (di != null)
            {
                if (di.FullName.Equals(diBase.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                else
                {
                    if (string.IsNullOrEmpty(subDirectory))
                    {
                        subDirectory = di.Name;
                    }
                    else
                    {
                        subDirectory = Path.Combine(di.Name, subDirectory);
                    }
                    di = di.Parent;
                }
            }
            return false;
        }
    }
}
