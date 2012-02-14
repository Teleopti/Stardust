using System.Windows.Forms;

namespace Teleopti.Ccc.AgentPortalCode.Common
{
    public static class MessageDialogs
    {
        public static void ShowWarning(Control owner, string message, string caption)
        {
            alert(owner);

            Syncfusion.Windows.Forms.MessageBoxAdv.Show(
                owner,
                message,
                caption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                (owner.RightToLeft == RightToLeft.Yes)
                    ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                    : 0);

        }

        public static void ShowError(Control owner, string message, string caption)
        {
            alert(owner);

            Syncfusion.Windows.Forms.MessageBoxAdv.Show(
                owner,
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
}
