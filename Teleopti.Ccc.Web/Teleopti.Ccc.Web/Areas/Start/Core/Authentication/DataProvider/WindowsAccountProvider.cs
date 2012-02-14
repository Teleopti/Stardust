using System.Web;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider
{
	public class WindowsAccountProvider : IWindowsAccountProvider
	{
		private readonly HttpContextBase _httpContextBase;
		
		public WindowsAccountProvider(HttpContextBase httpContextBase)
		{
			_httpContextBase = httpContextBase;
		}

		public WindowsAccount RetrieveWindowsAccount()
		{
			// err-hantering + test för detta senare... + browserstöd.
			var splitBySeparator = _httpContextBase.Request.ServerVariables["LOGON_USER"].Split('\\');
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