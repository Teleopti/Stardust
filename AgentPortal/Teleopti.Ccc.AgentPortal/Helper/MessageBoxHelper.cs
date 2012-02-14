#region Imports

using System;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers;

#endregion

namespace Teleopti.Ccc.AgentPortal.Helper
{

    /// <summary>
    /// Provides helper functionality for .
    /// </summary>
    public static class MessageBoxHelper
    {

        #region Constants

        #endregion

        #region Fields - Static Member

        #endregion

        #region Properties - Static Member

        #endregion

        #region Methods - Static Member

        public static void ShowErrorMessage(string text, string caption)
        {
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(string.Concat(text, "  "), caption,
                                                        MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                        MessageBoxDefaultButton.Button1,
                                                         CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
                                                            ? MessageBoxOptions.RtlReading |
                                                              MessageBoxOptions.RightAlign
                                                            : 0);
        }


        public static void ShowWarningMessage(string text, string caption)
        {
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(string.Concat(text, "  "), caption,
                                                        MessageBoxButtons.OK, MessageBoxIcon.Warning,
                                                        MessageBoxDefaultButton.Button1,
                                                         CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
                                                            ? MessageBoxOptions.RtlReading |
                                                              MessageBoxOptions.RightAlign
                                                            : 0);
        }

        public static void ShowWarningMessage(IWin32Window owner, string text, string caption)
        {
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(owner, string.Concat(text, "  "), caption,
                                                        MessageBoxButtons.OK, MessageBoxIcon.Warning,
                                                        MessageBoxDefaultButton.Button1,
                                                         CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
                                                            ? MessageBoxOptions.RtlReading |
                                                              MessageBoxOptions.RightAlign
                                                            : 0);
        }

        public static void ShowInformationMessage(string text, string caption)
        {
            Syncfusion.Windows.Forms.MessageBoxAdv.Show(string.Concat(text, "  "), caption,
                                                        MessageBoxButtons.OK, MessageBoxIcon.Information,
                                                        MessageBoxDefaultButton.Button1,
                                                        CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
                                                            ? MessageBoxOptions.RtlReading |
                                                              MessageBoxOptions.RightAlign
                                                            : 0);
        }

        public static DialogResult ShowConfirmationMessage(string text, string caption)
        {
            return Syncfusion.Windows.Forms.MessageBoxAdv.Show(string.Concat(text, "  "), caption,
                                                               MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                                               MessageBoxDefaultButton.Button1,
                                                                 CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
                                                                   ? MessageBoxOptions.RtlReading |
                                                                     MessageBoxOptions.RightAlign
                                                                   : 0);
        }

        #endregion

    }

}
