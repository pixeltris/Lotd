using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Lotd.FileFormats
{
    public class Dfymoo : FileData
    {
        private Encoding encoding = Encoding.ASCII;

        public string ImageType { get; set; }

        public bool HasWidth { get; set; }
        public int Width { get; set; }

        public bool HasHeight { get; set; }
        public int Height { get; set; }

        public Dictionary<string, Item> Items { get; private set; }

        public Dfymoo()
        {
            Items = new Dictionary<string, Item>();
        }

        public override void Load(BinaryReader reader, long length)
        {
            string n = null;
            string s = null;
            string o = null;

            string i = null;
            string w = null;
            string h = null;

            using (StreamReader streamReader = new StreamReader(new MemoryStream(reader.ReadBytes(length)), encoding))
            {
                string line = null;
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.Trim() == "~")
                    {
                        if (n != null)
                        {
                            Item item = new Item(n);

                            if (s != null)
                            {
                                string[] splitted = s.Split();
                                item.X = int.Parse(splitted[0]);
                                item.Y = int.Parse(splitted[1]);
                                item.Width = int.Parse(splitted[2]);
                                item.Height = int.Parse(splitted[3]);
                                item.HasSourceRect = true;
                            }

                            if (o != null)
                            {
                                string[] splitted = o.Split();
                                item.OffsetSizeX = int.Parse(splitted[0]);
                                item.OffsetSizeY = int.Parse(splitted[1]);
                                item.OffsetSizeWidth = int.Parse(splitted[2]);
                                item.OffsetSizeHeight = int.Parse(splitted[3]);
                                item.HasOffsetSize = true;
                            }

                            Items.Add(item.Name, item);
                        }
                        else if (i != null)
                        {
                            ImageType = i;
                            if (w != null)
                            {
                                Width = int.Parse(w);
                                HasWidth = true;
                            }
                            if (h != null)
                            {
                                Height = int.Parse(h);
                                HasHeight = true;
                            }
                        }

                        n = s = o = i = w = h = null;
                    }
                    else if (line.Length > 1)
                    {
                        char firstChar = line[0];
                        line = line.Substring(2);
                        switch (firstChar)
                        {
                            case 'n': n = line; break;
                            case 'o': o = line; break;
                            case 's': s = line; break;

                            case 'i': i = line; break;
                            case 'w': w = line; break;
                            case 'h': h = line; break;
                        }
                    }
                }
            }
        }

        public override void Save(BinaryWriter writer)
        {
            // Create a temporary stream as StreamReader/StreamWriter sometimes expand the buffer too much
            // - It is likely we don't need to do this for StreamWriter but for now write to a seperate stream
            //   until we know for sure this doesn't mess up the base stream position / length
            using (MemoryStream stream = new MemoryStream())
            using (StreamWriter streamWriter = new StreamWriter(stream, encoding))
            {
                streamWriter.WriteLine("i " + ImageType);
                if (HasWidth)
                {
                    streamWriter.WriteLine("w " + Width);
                }
                if (HasHeight)
                {
                    streamWriter.WriteLine("h " + Height);
                }
                streamWriter.WriteLine("~");

                foreach (Item item in Items.Values)
                {
                    streamWriter.WriteLine("n " + item.Name);
                    if (item.HasSourceRect)
                    {
                        streamWriter.WriteLine("s " + item.X + " " + item.Y + " " + item.Width + " " + item.Height);
                    }
                    if (item.HasOffsetSize)
                    {
                        streamWriter.WriteLine("o " + item.OffsetSizeX + " " + item.OffsetSizeY + " " + item.OffsetSizeWidth + " " + item.OffsetSizeHeight);
                    }
                    streamWriter.WriteLine("~");
                }

                streamWriter.Flush();
                writer.Write(stream.ToArray());
            }
        }

        public override void Clear()
        {
            Items.Clear();
        }

        public class Item
        {
            public string Name { get; set; }

            public bool HasSourceRect { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            // The desired size of the new rect is different than the source rect.
            // OffsetSizeX/Y = image offset into the new rect size
            // OffsetSizeWidth/Height = new rect size
            public bool HasOffsetSize { get; set; }
            public int OffsetSizeX { get; set; }
            public int OffsetSizeY { get; set; }
            public int OffsetSizeWidth { get; set; }
            public int OffsetSizeHeight { get; set; }

            public Item(string name)
            {
                Name = name;
            }
        }
    }
}
