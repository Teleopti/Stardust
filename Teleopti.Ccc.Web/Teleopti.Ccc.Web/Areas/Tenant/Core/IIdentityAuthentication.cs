namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public interface IIdentityAuthentication
	{
		ApplicationAuthenticationResult Logon(string identity);
	}
}