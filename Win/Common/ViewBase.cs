using System.Windows.Forms;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.WinCode.Common;

namespace Teleopti.Ccc.Win.Common
{
    public abstract class ViewBase<TPresenter> : IViewBase
    {
        private TPresenter _presenter;

        protected ViewBase(TPresenter presenter)
        {
            _presenter = presenter;
        }

        public TPresenter Presenter
        {
            get { return _presenter; }
            set
            {
                _presenter = value;
                OnPresenterSet();
            }
        }

        protected virtual void OnPresenterSet()
        {
        }

        public void ShowErrorMessage(string text,string caption)
        {
            ViewBase.ShowErrorMessage(text, caption);
        }

        public DialogResult ShowConfirmationMessage(string text, string caption)
        {
            return ViewBase.ShowConfirmationMessage(text, caption);
        }

        public DialogResult ShowYesNoMessage(string text, string caption)
		{
			return ViewBase.ShowYesNoMessage(text, caption);
		}

        public void ShowInformationMessage(string text, string caption)
        {
            ViewBase.ShowInformationMessage(text, caption);
        }

        public DialogResult ShowOkCancelMessage(string text, string caption)
        {
            return ViewBase.ShowOkCancelMessage(text, caption);
        }

        public DialogResult ShowWarningMessage(string text, string caption)
        {
            return ViewBase.ShowWarningMessage(text, caption);
        }

       
    }

    public static class ViewBase
    {
		private const string DoubleSpace = "";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
        public static void ShowErrorMessage(string text, string caption)
        {
			MessageBox.Show(string.Concat(text, DoubleSpace), caption,
                                                        MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                        MessageBoxDefaultButton.Button1,
                                                        (((IUnsafePerson)TeleoptiPrincipal.Current).Person.PermissionInformation.RightToLeftDisplay)
                                                            ? MessageBoxOptions.RtlReading |
                                                              MessageBoxOptions.RightAlign
                                                            : 0);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
        public static void ShowInformationMessage(string text, string caption)
        {
            MessageBox.Show(string.Concat(text, DoubleSpace), caption,
                                                        MessageBoxButtons.OK, MessageBoxIcon.Information,
                                                        MessageBoxDefaultButton.Button1,
                                                        (((IUnsafePerson)TeleoptiPrincipal.Current).Person.PermissionInformation.RightToLeftDisplay)
                                                            ? MessageBoxOptions.RtlReading |
                                                              MessageBoxOptions.RightAlign
                                                            : 0);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
        public static void ShowInformationMessage(IWin32Window owner, string text, string caption)
        {
            MessageBox.Show(owner, string.Concat(text, DoubleSpace), caption,
                                                        MessageBoxButtons.OK, MessageBoxIcon.Information,
                                                        MessageBoxDefaultButton.Button1,
                                                        (((IUnsafePerson)TeleoptiPrincipal.Current).Person.PermissionInformation.RightToLeftDisplay)
                                                            ? MessageBoxOptions.RtlReading |
                                                              MessageBoxOptions.RightAlign
                                                            : 0);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
        public static DialogResult ShowYesNoMessage(string text, string caption)
		{
            return MessageBox.Show(string.Concat(text, DoubleSpace), caption,
															   MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2,
                                                               (((IUnsafePerson)TeleoptiPrincipal.Current).Person.PermissionInformation.
																   RightToLeftDisplay)
																   ? MessageBoxOptions.RtlReading |
																	 MessageBoxOptions.RightAlign
																   : 0);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
        public static DialogResult ShowYesNoMessage(IWin32Window owner, string text, string caption)
        {
            return MessageBox.Show(owner, string.Concat(text, DoubleSpace), caption,
                                                               MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2,
                                                               (((IUnsafePerson)TeleoptiPrincipal.Current).Person.PermissionInformation.
                                                                   RightToLeftDisplay)
                                                                   ? MessageBoxOptions.RtlReading |
                                                                     MessageBoxOptions.RightAlign
                                                                   : 0);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
        public static DialogResult ShowConfirmationMessage(string text, string caption)
        {
            return MessageBox.Show(string.Concat(text, DoubleSpace), caption,
                                                               MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                                                               MessageBoxDefaultButton.Button1,
                                                               (((IUnsafePerson)TeleoptiPrincipal.Current).Person.PermissionInformation.
                                                                   RightToLeftDisplay)
                                                                   ? MessageBoxOptions.RtlReading |
                                                                     MessageBoxOptions.RightAlign
                                                                   : 0);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
		public static DialogResult ShowConfirmationMessage(IWin32Window owner, string text, string caption)
		{
            return MessageBox.Show(owner, string.Concat(text, DoubleSpace), caption,
															   MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
															   MessageBoxDefaultButton.Button1,
                                                               (((IUnsafePerson)TeleoptiPrincipal.Current).Person.PermissionInformation.
																   RightToLeftDisplay)
																   ? MessageBoxOptions.RtlReading |
																	 MessageBoxOptions.RightAlign
																   : 0);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
        public static DialogResult ShowOkCancelMessage(string text, string caption)
        {
            return MessageBox.Show(string.Concat(text, DoubleSpace), caption,
                                                               MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
                                                               MessageBoxDefaultButton.Button1,
                                                               (((IUnsafePerson)TeleoptiPrincipal.Current).Person.PermissionInformation.
                                                                   RightToLeftDisplay)
                                                                   ? MessageBoxOptions.RtlReading |
                                                                     MessageBoxOptions.RightAlign
                                                                   : 0);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
        public static DialogResult ShowWarningMessage(string text, string caption)
        {
            return MessageBox.Show(string.Concat(text, DoubleSpace), caption,
                MessageBoxButtons.OK, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
                (((IUnsafePerson)TeleoptiPrincipal.Current).Person.PermissionInformation.
                    RightToLeftDisplay)
                    ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign
                    : 0);
        }
    }
}
