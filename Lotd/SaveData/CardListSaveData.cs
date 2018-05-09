using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Lotd
{
    public class CardListSaveData : SaveDataChunk
    {
        public CardState[] Cards { get; private set; }

        public CardListSaveData()
        {
            Cards = new CardState[Constants.NumCards];
        }

        public override void Clear()
        {
            for (int i = 0; i < Constants.NumCards; i++)
            {
                Cards[i] = CardState.None;
            }
        }

        public override void Load(BinaryReader reader)
        {
            for (int i = 0; i < Constants.NumCards; i++)
            {
                Cards[i].RawValue = reader.ReadByte();
            }
        }

        public override void Save(BinaryWriter writer)
        {
            for (int i = 0; i < Constants.NumCards; i++)
            {
                writer.Write(Cards[i].RawValue);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CardState
    {
        public byte RawValue;

        // xxxxx111 - number of cards owned (offset:0 bits:3 mask:7)
        // xxxx1xxx - 0="NEW" 1=seen card (offset:3 bits:1 mask:1)
        // 1111xxxx - ? (offset:4 bits:4 mask:0xF)

        /// <summary>
        /// Number of cards owned (0-3)
        /// </summary>
        public byte Count
        {
            get { return (byte)(RawValue & 7); }
            set
            {
                // Mask out the existing value
                RawValue &= 0xF8;

                // Set the new value
                RawValue |= (byte)(value & 7);
            }
        }

        /// <summary>
        /// If false this card will have a "NEW" marker on it
        /// </summary>
        public bool Seen
        {
            get { return ((RawValue >> 3) & 1) != 0; }
            set
            {
                // Mask out the existing value
                RawValue &= 0xF7;

                // Set the new value
                if (value)
                {
                    RawValue |= (byte)(1 << 3);
                }
            }
        }

        public byte Unkown
        {
            get { return (byte)(RawValue >> 4); }
            set
            {
                // Mask out the existing value
                RawValue &= 0xF;

                // Set the new value
                RawValue |= (byte)((value & 0xF) << 4);
            }
        }

        public static CardState None
        {
            get { return default(CardState); }
        }
    }
}
