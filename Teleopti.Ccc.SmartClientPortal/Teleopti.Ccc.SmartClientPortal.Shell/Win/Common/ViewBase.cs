using System;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
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

        public DialogResult ShowWarningMessage(string text, string caption)
        {
            return ViewBase.ShowWarningMessage(text, caption);
        }

       
    }

    public static class ViewBase
    {
		private const string DoubleSpace = "  ";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBoxAdv.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
        public static void ShowErrorMessage(string text, string caption)
        {
			LocalizationProvider.Provider = new Localizer();
			MessageBoxAdv.Show(string.Concat(text, DoubleSpace), caption,
                                                        MessageBoxButtons.OK, MessageBoxIcon.Error,
                                                        MessageBoxDefaultButton.Button1,
                                                        rightToLeftOptions());
        }

	    private static MessageBoxOptions rightToLeftOptions()
	    {
		    return (((ITeleoptiPrincipalForLegacy)TeleoptiPrincipalForLegacy.CurrentPrincipal).UnsafePerson.PermissionInformation.RightToLeftDisplay)
			           ? MessageBoxOptions.RtlReading |
			             MessageBoxOptions.RightAlign
			           : 0;
	    }

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.MessageBoxAdv.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBoxAdv.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
		public static void ShowErrorMessage(IWin32Window owner, string text, string caption)
		{
			LocalizationProvider.Provider = new Localizer();
            MessageBoxAdv.Show(new WeakOwner(owner), string.Concat(text, DoubleSpace), caption,
														MessageBoxButtons.OK, MessageBoxIcon.Error,
														MessageBoxDefaultButton.Button1,
														rightToLeftOptions());
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBoxAdv.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
        public static void ShowInformationMessage(string text, string caption)
        {
			LocalizationProvider.Provider = new Localizer();
            MessageBoxAdv.Show(string.Concat(text, DoubleSpace), caption,
                                                        MessageBoxButtons.OK, MessageBoxIcon.Information,
                                                        MessageBoxDefaultButton.Button1,
														rightToLeftOptions());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBoxAdv.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
        public static void ShowInformationMessage(IWin32Window owner, string text, string caption)
        {
			LocalizationProvider.Provider = new Localizer();
            MessageBoxAdv.Show(new WeakOwner(owner), string.Concat(text, DoubleSpace), caption,
                                                        MessageBoxButtons.OK, MessageBoxIcon.Information,
                                                        MessageBoxDefaultButton.Button1,
														rightToLeftOptions());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBoxAdv.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
        public static DialogResult ShowYesNoMessage(string text, string caption)
		{
			LocalizationProvider.Provider = new Localizer();
            return MessageBoxAdv.Show(string.Concat(text, DoubleSpace), caption,
															   MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2,
															   rightToLeftOptions());
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBoxAdv.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
        public static DialogResult ShowYesNoMessage(IWin32Window owner, string text, string caption)
        {
            return ShowYesNoMessage(new WeakOwner(owner), text, caption, MessageBoxDefaultButton.Button2);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.MessageBoxAdv.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
		public static DialogResult ShowYesNoMessage(IWin32Window owner, string text, string caption, MessageBoxDefaultButton defaultButton)
		{
			LocalizationProvider.Provider = new Localizer();
			return MessageBoxAdv.Show(new WeakOwner(owner), string.Concat(text, DoubleSpace), caption,
															   MessageBoxButtons.YesNo, MessageBoxIcon.Question, defaultButton,
															   rightToLeftOptions());
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBoxAdv.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
        public static DialogResult ShowConfirmationMessage(string text, string caption)
        {
			LocalizationProvider.Provider = new Localizer();
            return MessageBoxAdv.Show(string.Concat(text, DoubleSpace), caption,
                                                               MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                                                               MessageBoxDefaultButton.Button1,
															   rightToLeftOptions());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBoxAdv.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
		public static DialogResult ShowConfirmationMessage(IWin32Window owner, string text, string caption)
		{
			LocalizationProvider.Provider = new Localizer();
            return MessageBoxAdv.Show(new WeakOwner(owner), string.Concat(text, DoubleSpace), caption,
															   MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
															   MessageBoxDefaultButton.Button1,
															   rightToLeftOptions());
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
                get {
                    return _window.IsAlive ? ((IWin32Window) _window.Target).Handle : IntPtr.Zero;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBoxAdv.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
        public static DialogResult ShowOkCancelMessage(string text, string caption)
        {
			LocalizationProvider.Provider = new Localizer();
            return MessageBoxAdv.Show(string.Concat(text, DoubleSpace), caption,
                                                               MessageBoxButtons.OKCancel, MessageBoxIcon.Question,
                                                               MessageBoxDefaultButton.Button1,
															   rightToLeftOptions());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBoxAdv.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
        public static DialogResult ShowWarningMessage(string text, string caption)
        {
			LocalizationProvider.Provider = new Localizer();
            return MessageBoxAdv.Show(string.Concat(text, DoubleSpace), caption,
                MessageBoxButtons.OK, MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button1,
				rightToLeftOptions());
        }

		public static void ShowHelp(IHelpForm owner, bool local)
		{
			ColorHelper guiHelper = new ColorHelper();
			var activeControl = guiHelper.GetActiveControl(owner as Control);
			IHelpContext userControl = null;
			while (activeControl != null)
			{
				if (tryGetHelpFromContext(activeControl, ref userControl)) break;
				if (tryGetHelpFromManualContext(owner, activeControl, ref userControl)) break;

				var panel = activeControl as Panel;
				if (panel != null && panel.Controls.Count > 0)
				{
					if (tryGetHelpFromContext(panel.Controls[0], ref userControl)) break;
				}

				activeControl = activeControl.Parent;
			}

			HelpHelper.Current.GetHelp(owner, userControl, local);
		}

		private static bool tryGetHelpFromContext(Control activeControl, ref IHelpContext userControl)
		{
			userControl = activeControl as IHelpContext;
			if (userControl != null && userControl.HasHelp) return true;
			return false;
		}

		private static bool tryGetHelpFromManualContext(IHelpForm form, Control activeControl, ref IHelpContext userControl)
		{
			userControl = form.FindMatchingManualHelpContext(activeControl);
			if (userControl != null && userControl.HasHelp) return true;
			return false;
		}
    }
}
