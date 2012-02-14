namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider
{
	public class WindowsAccount
	{
		public WindowsAccount(string domainName, string userName)
		{
			DomainName = domainName;
			UserName = userName;
		}

		public string DomainName { get; private set; }
		public string UserName { get; private set; }
	}
}