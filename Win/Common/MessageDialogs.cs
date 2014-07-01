using System.Windows.Forms;
using Syncfusion.Windows.Forms;

namespace Teleopti.Ccc.Win.Common
{
    public static class MessageDialogs
    {
        public static void ShowWarning(Control owner, string message, string caption)
        {
            alert(owner);

            MessageBoxAdv.Show(
                new ViewBase.WeakOwner(owner),
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

            MessageBoxAdv.Show(
                new ViewBase.WeakOwner(owner),
                message,
                caption,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1,
                (owner.RightToLeft == RightToLeft.Yes)
                    ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                    : 0);
            
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static void ShowInformation(Control owner, string message, string caption)
		{
			alert(owner);

			MessageBoxAdv.Show(
                new ViewBase.WeakOwner(owner),
				message,
				caption,
				MessageBoxButtons.OK,
				MessageBoxIcon.Information,
				MessageBoxDefaultButton.Button1,
				(owner.RightToLeft == RightToLeft.Yes)
					? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
					: 0);

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static DialogResult ShowQuestion(Control owner, string message, string caption)
		{
			alert(owner);

			return MessageBoxAdv.Show(owner,
				   message,
				   caption,
				   MessageBoxButtons.YesNo,
				   MessageBoxIcon.Question,
				   MessageBoxDefaultButton.Button2,
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
