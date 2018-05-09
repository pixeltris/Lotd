using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd
{
    /// <summary>
    /// Abstract class which holds information about a given chunk within a game save data file
    /// </summary>
    public abstract class SaveDataChunk
    {
        public virtual void Clear()
        {
        }

        public virtual void Load(byte[] buffer)
        {
            using (BinaryReader reader = new BinaryReader(new MemoryStream(buffer)))
            {
                Load(reader);
            }
        }

        public virtual void Load(BinaryReader reader)
        {
        }

        public virtual byte[] Save()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                Save(writer);
                return stream.ToArray();
            }
        }

        public virtual void Save(BinaryWriter writer)
        {
        }
    }
}
