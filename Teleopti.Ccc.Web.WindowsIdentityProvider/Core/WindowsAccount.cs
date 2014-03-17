namespace Teleopti.Ccc.Web.WindowsIdentityProvider.Core
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