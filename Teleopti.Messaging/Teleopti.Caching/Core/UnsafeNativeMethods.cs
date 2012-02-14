using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Teleopti.Caching.Core
{
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors"),
     SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"),
     SuppressUnmanagedCodeSecurity]
    public sealed class UnsafeNativeMethods
    {
        internal static readonly IntPtr InvalidHandleValue = new IntPtr(-1);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "4"),
         DllImport("kernel32", SetLastError = true)]
        internal static extern unsafe byte* MapViewOfFile(IntPtr mappingHandle, int dwAccess, int fileOffsetHi, int fileOffsetLo, int numBytes);

        [SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"),
         DllImport("kernel32", SetLastError = true)]
        internal static extern bool SetEndOfFile(IntPtr fileHandle);

        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "0"),
         DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CreateFile(string fileName, int dwDesiredAccess, FileShare shareMode, IntPtr lpSecurityAttributes_MustBeZero, int dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile_MustBeZero);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"),
         SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"),
         DllImport("kernel32", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr fileHandle);

        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "5"), DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CreateFileMapping(IntPtr fileHandle, IntPtr lpSecurityAttributes_MustBeNull, int dwProtect, int maxSizeHi, int maxSizeLo, string name);

        [SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"),
         DllImport("kernel32", SetLastError = true)]
        internal static extern bool SetFilePointer(IntPtr fileHandle, int lo, ref int hi, SeekOrigin origin);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"),
         DllImport("kernel32", SetLastError = true)]
        internal static extern int GetFileSize(IntPtr fileHandle, ref int hi);

        #pragma warning disable 675

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage")]
        public static long GetLengthOfFile(IntPtr fileHandle)
        {
            int pHi = 0;
            return (((long)((ulong)GetFileSize(fileHandle, ref pHi))) | (pHi << 0x20));
        }

        #pragma warning restore 675

        [SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "6"),
        SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "4"),
        SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "1"),
        SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage"),
        DllImport("kernel32", CharSet = CharSet.Auto)]
        internal static extern int FormatMessage(int flags, string source, int messageID, int languageID, StringBuilder buffer, int bufferSize, string insert0);

        // Can be deleted

        [SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs"),
         SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"),
         DllImport("kernel32", SetLastError = true)]
        internal static extern unsafe bool UnmapViewOfFile(byte* memory);

    }
}
