using System.Globalization;
using System.Windows.Forms;

namespace Teleopti.Ccc.AgentPortal.Helper
{
    public static class MessageBoxHelper
    {
    	private const string DoubleSpaces = "";

        public static void ShowErrorMessage(string text, string caption)
        {
            MessageBox.Show(string.Concat(text, DoubleSpaces), caption,
                                                        MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                        MessageBoxDefaultButton.Button1,
                                                         CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
                                                            ? MessageBoxOptions.RtlReading |
                                                              MessageBoxOptions.RightAlign
                                                            : 0);
        }

        public static void ShowWarningMessage(string text, string caption)
        {
			MessageBox.Show(string.Concat(text, DoubleSpaces), caption,
                                                        MessageBoxButtons.OK, MessageBoxIcon.Warning,
                                                        MessageBoxDefaultButton.Button1,
                                                         CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
                                                            ? MessageBoxOptions.RtlReading |
                                                              MessageBoxOptions.RightAlign
                                                            : 0);
        }

        public static void ShowWarningMessage(IWin32Window owner, string text, string caption)
        {
			MessageBox.Show(owner, string.Concat(text, DoubleSpaces), caption,
                                                        MessageBoxButtons.OK, MessageBoxIcon.Warning,
                                                        MessageBoxDefaultButton.Button1,
                                                         CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
                                                            ? MessageBoxOptions.RtlReading |
                                                              MessageBoxOptions.RightAlign
                                                            : 0);
        }

        public static void ShowInformationMessage(string text, string caption)
        {
			MessageBox.Show(string.Concat(text, DoubleSpaces), caption,
                                                        MessageBoxButtons.OK, MessageBoxIcon.Information,
                                                        MessageBoxDefaultButton.Button1,
                                                        CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
                                                            ? MessageBoxOptions.RtlReading |
                                                              MessageBoxOptions.RightAlign
                                                            : 0);
        }

        public static DialogResult ShowConfirmationMessage(string text, string caption)
        {
			return MessageBox.Show(string.Concat(text, DoubleSpaces), caption,
                                                               MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                                               MessageBoxDefaultButton.Button1,
                                                                 CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
                                                                   ? MessageBoxOptions.RtlReading |
                                                                     MessageBoxOptions.RightAlign
                                                                   : 0);
        }
    }
}
