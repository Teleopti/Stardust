using System.Web;

namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Core
{
	public class WindowsAccountProvider : IWindowsAccountProvider
	{
		public WindowsAccount RetrieveWindowsAccount()
		{
			// err-hantering + test för detta senare... + browserstöd.
			var splitBySeparator = HttpContext.Current.Request.ServerVariables["LOGON_USER"].Split('\\');
			if(splitBySeparator.Length != 2)
			{
				return null;
			}

			var domainName = splitBySeparator[0];
			var userName = splitBySeparator[1];
			return new WindowsAccount(domainName, userName);
		}
	}
}