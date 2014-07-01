using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    public static class MessageDialogs
    {
        public static void ShowError(Control owner, string message, string caption)
        {
            alert(owner);

            MessageBoxAdv.Show(
                new WeakOwner(owner),
                message,
                caption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                (owner.RightToLeft == RightToLeft.Yes)
                    ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                    : 0);

        }

        private static void alert(Control owner)
        {
            Form form = owner as Form;
            if (form != null)
            {
                form.Activate();
            }
        }
    }

    public class WeakOwner : IWin32Window
    {
        private readonly WeakReference _window;

        public WeakOwner(IWin32Window window)
        {
            _window = new WeakReference(window);
        }

        public IntPtr Handle
        {
            get
            {
                return _window.IsAlive ? ((IWin32Window)_window.Target).Handle : IntPtr.Zero;
            }
        }
    }
}
