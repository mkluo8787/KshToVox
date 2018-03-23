using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace LZ77
{
    public static class LZ77Agent
    {
        public static byte[] Compress(byte[] input)
        {
            List<byte> output = new List<byte>();
                        
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] input)
        {
            List<byte> output = new List<byte>();

            return output.ToArray();
        }

        public static byte[] HexToByte(string input)
        {
            return Enumerable.Range(0, input.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(input.Substring(x, 2), 16))
                     .ToArray();
        }
    }
}
