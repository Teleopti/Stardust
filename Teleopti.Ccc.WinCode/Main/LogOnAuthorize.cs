using System.Windows.Forms;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.WinCode.Main
{
	public static class LogonAuthorize
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.MessageBox.Show(System.Windows.Forms.IWin32Window,System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
		public static bool Authorize()
		{
			using (PerformanceOutput.ForOperation("Authorize including loading permissions"))
			{
				if (!PrincipalAuthorization.Instance().IsPermitted(
						 DefinedRaptorApplicationFunctionPaths.OpenRaptorApplication))
				{
					MessageBox.Show(null,
						string.Concat(UserTexts.Resources.YouAreNotAuthorizedToRunTheApplication, "  "),
						UserTexts.Resources.AuthenticationFailed, MessageBoxButtons.OK, MessageBoxIcon.Warning,
						MessageBoxDefaultButton.Button1, 0);
					return false;
				}

				return true;
			}
		}
	}
}
