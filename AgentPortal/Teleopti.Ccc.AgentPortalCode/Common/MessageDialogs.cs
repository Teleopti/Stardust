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
