using System.Windows.Forms;

namespace Teleopti.Support.Tool.Controls
{
    public static class ThreadSafeControlDelegation
    {

        public static void SetEnabled(bool state, Control control)
        {
            if (control != null && control.InvokeRequired)
            {
                control.BeginInvoke(
                    new MethodInvoker(
                    delegate() { SetEnabled(state, control); }));
            }
            else
            {
                control.Enabled = state;
            }
        }


        public static void SetCursor(Cursor cursor, Control control)
        {
            if (control != null && control.InvokeRequired)
            {
                control.BeginInvoke(
                    new MethodInvoker(
                    delegate() { SetCursor(cursor, control); }));
            }
            else
            {
                control.Cursor = cursor;
            }
        }
    }
}
