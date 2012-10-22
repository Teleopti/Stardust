using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Teleopti.Support.Tool.Controls
{
    public class ThreadInputAttachment : IDisposable
    {
        private readonly int _targetThreadId;
        private int _callingThreadID;
        private bool _mustDetach;

        [DllImport("kernel32.dll")]
        private static extern int GetCurrentThreadId();

        [DllImport("User32.dll")]
        private static extern int AttachThreadInput(
            int idAttach, 
            int idAttachTo, 
            [MarshalAs(UnmanagedType.Bool)] bool fAttach);

        private ThreadInputAttachment() { }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.ToString")]
        public ThreadInputAttachment(int targetThreadId) : this()
        {
            _targetThreadId = targetThreadId;
            _callingThreadID = GetCurrentThreadId();
            //System.Threading.Thread.CurrentThread.ManagedThreadId
            if (targetThreadId != _callingThreadID)
            {
                if (AttachThreadInput(_targetThreadId, _callingThreadID, true) == 0)
                {
                    int lastWin32ErrorCore = Marshal.GetLastWin32Error();
                    StringBuilder win32ErrorMessage = new StringBuilder();
                    win32ErrorMessage.Append("AttachThreadInput failed, GetLastWin32Error() returned: ");
                    win32ErrorMessage.Append(lastWin32ErrorCore.ToString(CultureInfo.InvariantCulture));
                    throw new Win32Exception(win32ErrorMessage.ToString());
                }
                _mustDetach = true;
            }
        }

        public void Dispose()
        {
            if (_mustDetach)
            {
                AttachThreadInput(_targetThreadId, _callingThreadID, false);
            }
        }
    }
}
