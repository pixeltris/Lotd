using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lotd
{
    public static class Endian
    {
        public static bool IsLittleEndian
        {
            get { return BitConverter.IsLittleEndian; }
        }

        public static short ConvertInt16(short value)
        {
            return System.Net.IPAddress.HostToNetworkOrder(value);
        }

        public static ushort ConvertUInt16(ushort value)
        {
            return (ushort)System.Net.IPAddress.HostToNetworkOrder((short)value);
        }

        public static int ConvertInt32(int value)
        {
            return System.Net.IPAddress.HostToNetworkOrder(value);
        }

        public static uint ConvertUInt32(uint value)
        {
            return (uint)System.Net.IPAddress.HostToNetworkOrder((int)value);
        }

        public static long ConvertInt64(long value)
        {
            return System.Net.IPAddress.HostToNetworkOrder(value);
        }

        public static ulong ConvertUInt64(ulong value)
        {
            return (ulong)System.Net.IPAddress.HostToNetworkOrder((long)value);
        }

        public static float ConvertSingle(float value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            Array.Reverse(buffer);
            return BitConverter.ToSingle(buffer, 0);
        }

        public static double ConvertDouble(double value)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            Array.Reverse(buffer);
            return BitConverter.ToDouble(buffer, 0);
        }

        public static short ToInt16(byte[] value, int startIndex)
        {
            return ToInt16(value, startIndex, IsLittleEndian);
        }

        public static short ToInt16(byte[] value, int startIndex, bool convert)
        {
            short result = BitConverter.ToInt16(value, startIndex);
            return convert ? ConvertInt16(result) : result;
        }

        public static ushort ToUInt16(byte[] value, int startIndex)
        {
            return ToUInt16(value, startIndex, IsLittleEndian);
        }

        public static ushort ToUInt16(byte[] value, int startIndex, bool convert)
        {
            ushort result = BitConverter.ToUInt16(value, startIndex);
            return convert ? ConvertUInt16(result) : result;
        }

        public static int ToInt32(byte[] value, int startIndex)
        {
            return ToInt32(value, startIndex, IsLittleEndian);
        }

        public static int ToInt32(byte[] value, int startIndex, bool convert)
        {
            int result = BitConverter.ToInt32(value, startIndex);
            return convert ? ConvertInt32(result) : result;
        }

        public static uint ToUInt32(byte[] value, int startIndex)
        {
            return ToUInt32(value, startIndex, IsLittleEndian);
        }

        public static uint ToUInt32(byte[] value, int startIndex, bool convert)
        {
            uint result = BitConverter.ToUInt32(value, startIndex);
            return convert ? ConvertUInt32(result) : result;
        }

        public static long ToInt64(byte[] value, int startIndex)
        {
            return ToInt64(value, startIndex, IsLittleEndian);
        }

        public static long ToInt64(byte[] value, int startIndex, bool convert)
        {
            long result = BitConverter.ToInt64(value, startIndex);
            return convert ? ConvertInt64(result) : result;
        }

        public static ulong ToUInt64(byte[] value, int startIndex)
        {
            return ToUInt64(value, startIndex, IsLittleEndian);
        }

        public static ulong ToUInt64(byte[] value, int startIndex, bool convert)
        {
            ulong result = BitConverter.ToUInt64(value, startIndex);
            return convert ? ConvertUInt64(result) : result;
        }

        public static float ToSingle(byte[] value, int startIndex)
        {
            return ToSingle(value, startIndex, IsLittleEndian);
        }

        public static float ToSingle(byte[] value, int startIndex, bool convert)
        {
            float result = BitConverter.ToSingle(value, startIndex);
            return convert ? ConvertSingle(result) : result;
        }

        public static double ToDouble(byte[] value, int startIndex)
        {
            return ToDouble(value, startIndex, IsLittleEndian);
        }

        public static double ToDouble(byte[] value, int startIndex, bool convert)
        {
            double result = BitConverter.ToDouble(value, startIndex);
            return convert ? ConvertDouble(result) : result;
        }

        public static byte[] GetBytes(short value)
        {
            return GetBytes(value, IsLittleEndian);
        }

        public static byte[] GetBytes(short value, bool convert)
        {
            byte[] result = BitConverter.GetBytes(value);
            if (convert) Array.Reverse(result);
            return result;
        }

        public static byte[] GetBytes(ushort value)
        {
            return GetBytes(value, IsLittleEndian);
        }

        public static byte[] GetBytes(ushort value, bool convert)
        {
            byte[] result = BitConverter.GetBytes(value);
            if (convert) Array.Reverse(result);
            return result;
        }

        public static byte[] GetBytes(int value)
        {
            return GetBytes(value, IsLittleEndian);
        }

        public static byte[] GetBytes(int value, bool convert)
        {
            byte[] result = BitConverter.GetBytes(value);
            if (convert) Array.Reverse(result);
            return result;
        }

        public static byte[] GetBytes(uint value)
        {
            return GetBytes(value, IsLittleEndian);
        }

        public static byte[] GetBytes(uint value, bool convert)
        {
            byte[] result = BitConverter.GetBytes(value);
            if (convert) Array.Reverse(result);
            return result;
        }

        public static byte[] GetBytes(long value)
        {
            return GetBytes(value, IsLittleEndian);
        }

        public static byte[] GetBytes(long value, bool convert)
        {
            byte[] result = BitConverter.GetBytes(value);
            if (convert) Array.Reverse(result);
            return result;
        }

        public static byte[] GetBytes(ulong value)
        {
            return GetBytes(value, IsLittleEndian);
        }

        public static byte[] GetBytes(ulong value, bool convert)
        {
            byte[] result = BitConverter.GetBytes(value);
            if (convert) Array.Reverse(result);
            return result;
        }

        public static byte[] GetBytes(float value)
        {
            return GetBytes(value, IsLittleEndian);
        }

        public static byte[] GetBytes(float value, bool convert)
        {
            byte[] result = BitConverter.GetBytes(value);
            if (convert) Array.Reverse(result);
            return result;
        }

        public static byte[] GetBytes(double value)
        {
            return GetBytes(value, IsLittleEndian);
        }

        public static byte[] GetBytes(double value, bool convert)
        {
            byte[] result = BitConverter.GetBytes(value);
            if (convert) Array.Reverse(result);
            return result;
        }
    }
}
