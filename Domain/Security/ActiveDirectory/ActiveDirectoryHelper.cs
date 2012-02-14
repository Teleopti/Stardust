using System;
using System.Globalization;
using System.Text;
using System.Runtime.InteropServices;
using Teleopti.Ccc.Domain.Security.ActiveDirectory;

namespace Teleopti.Ccc.Domain.Security.ActiveDirectory
{
    /// <summary>
    /// Various Methods Used in Active Directory.
    /// </summary>
    public static class ActiveDirectoryHelper
    {

        #region Interface

        /// <summary>
        /// Converts a Byte[] ObjectGUID to a Binary String Representation.
        /// </summary>
        /// <param name="ObjectGUID">The ObjectGUID in Bytes</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Object"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "GUID")]
        public static string ConvertBytesToStringGUID(Byte[] ObjectGUID)
        {
            StringBuilder sbGUID = new StringBuilder();

            // First Group XXXXXXXX-
            sbGUID.Append(
                string.Format(CultureInfo.InvariantCulture, "{0:X2}{1:X2}{2:X2}{3:X2}",
                    (Int16)ObjectGUID[0],
                    (Int16)ObjectGUID[1],
                    (Int16)ObjectGUID[2],
                    (Int16)ObjectGUID[3]));
            sbGUID.Append("-");

            // Second Group XXXX-
            sbGUID.Append(
                string.Format(CultureInfo.InvariantCulture, "{0:X2}{1:X2}",
                    (Int16)ObjectGUID[4],
                    (Int16)ObjectGUID[5]));
            sbGUID.Append("-");

            // Third Group XXXX-
            sbGUID.Append(
                string.Format(CultureInfo.InvariantCulture, "{0:X2}{1:X2}",
                    (Int16)ObjectGUID[6],
                    (Int16)ObjectGUID[7]));
            sbGUID.Append("-");

            // Fourth Group XXXX-
            sbGUID.Append(
                string.Format(CultureInfo.InvariantCulture, "{0:X2}{1:X2}",
                    (Int16)ObjectGUID[8],
                    (Int16)ObjectGUID[9]));
            sbGUID.Append("-");

            // Fifth Group XXXXXXXXXXXX-
            sbGUID.Append(
                string.Format(CultureInfo.InvariantCulture, "{0:X2}{1:X2}{2:X2}{3:X2}{4:X2}{5:X2}",
                    (Int16)ObjectGUID[10],
                    (Int16)ObjectGUID[11],
                    (Int16)ObjectGUID[12],
                    (Int16)ObjectGUID[13],
                    (Int16)ObjectGUID[14],
                    (Int16)ObjectGUID[15]));

            return sbGUID.ToString();
        }

        /// <summary>
        /// Converts a Byte[] ObjectSID to a Security String Representation.
        /// </summary>
        /// <param name="ObjectSID">The Object SID in Bytes</param>
        /// <returns>the ObjectSID in String Format</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Object"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SID")]
        public static string ConvertBytesToStringSid(Byte[] ObjectSID)
        {
            short sSubAuthorityCount = 0;
            StringBuilder sbSID = new StringBuilder();
            sbSID.Append("S-");
            // Add SID revision.
            sbSID.Append(ObjectSID[0].ToString(CultureInfo.InvariantCulture));

            sSubAuthorityCount = Convert.ToInt16(ObjectSID[1]);

            // Next six bytes are SID authority value.
            if (ObjectSID[2] != 0 || ObjectSID[3] != 0)
            {
                string strAuth = String.Format(CultureInfo.InvariantCulture, "0x{0:2x}{1:2x}{2:2x}{3:2x}{4:2x}{5:2x}",
                    (Int16)ObjectSID[2],
                    (Int16)ObjectSID[3],
                    (Int16)ObjectSID[4],
                    (Int16)ObjectSID[5],
                    (Int16)ObjectSID[6],
                    (Int16)ObjectSID[7]);
                sbSID.Append("-");
                sbSID.Append(strAuth);
            }
            else
            {
                Int64 iVal = ObjectSID[7] +
                    (ObjectSID[6] << 8) +
                    (ObjectSID[5] << 16) +
                    (ObjectSID[4] << 24);
                sbSID.Append("-");
                sbSID.Append(iVal.ToString(CultureInfo.InvariantCulture));
            }

            // Get sub authority count...
            int idxAuth = 0;
            for (int i = 0; i < sSubAuthorityCount; i++)
            {
                idxAuth = 8 + i * 4;
                UInt32 iSubAuth = BitConverter.ToUInt32(ObjectSID, idxAuth);
                sbSID.Append("-");
                sbSID.Append(iSubAuth.ToString(CultureInfo.InvariantCulture));
            }
            return sbSID.ToString();
        }

        #endregion

    }
}
