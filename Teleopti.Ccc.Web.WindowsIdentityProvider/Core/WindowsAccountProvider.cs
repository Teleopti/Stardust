namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Core
{
	public class WindowsAccountProvider : IWindowsAccountProvider
	{
		private readonly ICurrentHttpContext _currentHttpContext;

		public WindowsAccountProvider(ICurrentHttpContext currentHttpContext)
		{
			_currentHttpContext = currentHttpContext;
		}

		public WindowsAccount RetrieveWindowsAccount()
		{
			// err-hantering + test för detta senare... + browserstöd.
			var splitBySeparator = _currentHttpContext.Current().Request.ServerVariables["LOGON_USER"].Split('\\');
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