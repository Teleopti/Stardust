using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Teleopti.Caching.Core
{
    [SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
    public sealed class CachingError
    {
        private const string DirectoryNotFoundPath = "Could not find path {0}.";

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Caching.Core.UnsafeNativeMethods.FormatMessage(System.Int32,System.String,System.Int32,System.Int32,System.Text.StringBuilder,System.Int32,System.String)"), SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Messaging.Core.UnsafeNativeMethods.FormatMessage(System.Int32,System.String,System.Int32,System.Int32,System.Text.StringBuilder,System.Int32,System.String)"), SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Messaging.Caching.UnsafeNativeMethods.FormatMessage(System.Int32,System.String,System.Int32,System.Int32,System.Text.StringBuilder,System.Int32,System.String)")]
        private static string LookupWin32Error(int errorCode, string insert)
        {
            StringBuilder buffer = new StringBuilder(0x400);
            // ReSharper disable ConvertToConstant
            int languageID = 0;
            // ReSharper restore ConvertToConstant
            UnsafeNativeMethods.FormatMessage(0x3000, null, errorCode, languageID, buffer, buffer.Capacity, insert);
            return buffer.ToString();
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public static void WinIOError()
        {
            WinIOError(string.Empty, Marshal.GetLastWin32Error());
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public static void WinIOError(string message)
        {
            WinIOError(message, Marshal.GetLastWin32Error());
        }

        public static void WinIOError(string message, int errorCode)
        {
            int hresult = errorCode;
            if ((hresult & 0x80000000L) == 0L)
            {
                hresult = -2147024896 | errorCode;
            }
            switch (errorCode)
            {
                case 2:
                    throw new FileNotFoundException(LookupWin32Error(errorCode, message), message);

                case 3:
                    throw new DirectoryNotFoundException(String.Format(CultureInfo.InvariantCulture, DirectoryNotFoundPath, message));
            }
            throw new IOException(LookupWin32Error(errorCode, message), hresult);
        }
    }
}