namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IIdentityAuthentication
	{
		ApplicationAuthenticationResult Logon(string identity);
	}
}