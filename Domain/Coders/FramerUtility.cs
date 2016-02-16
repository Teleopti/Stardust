using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;

namespace Teleopti.Ccc.Domain.Coders
{
    public class FramerUtility : IFramerUtility
    {
        public byte[] NextToken(Stream input, byte[] delimiter)
        {
            int nextByte;
            // If the stream has already ended, return null
            if ((nextByte = input.ReadByte()) == -1)
                return null;
            List<byte> byteList = new List<byte>(Consts.MaxWireLength);
            do
            {
                byteList.Add((byte) nextByte);
                if (EndsWith(byteList, delimiter))
                {
                    byteList.RemoveRange(byteList.Count - delimiter.Length, delimiter.Length);
                    break;
                }
            } while ((nextByte = input.ReadByte()) != -1); // Stop on EOS
            if (byteList.Count == 0)
                return null;
            return byteList.ToArray(); // Received at least one byte
        }

        // Returns true if value ends with the bytes in the suffix
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public Boolean EndsWith(IList<byte> value, byte[] suffix)
        {
            if (value.Count < suffix.Length)
                return false;

            for (int offset = 1; offset <= suffix.Length; offset++)
                if (value[value.Count - offset] != suffix[suffix.Length - offset])
                    return false;

            return true;
        }
    }
}