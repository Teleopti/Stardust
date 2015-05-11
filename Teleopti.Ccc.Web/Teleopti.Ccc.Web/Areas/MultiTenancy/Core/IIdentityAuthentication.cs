namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IIdentityAuthentication
	{
		TenantAuthenticationResult Logon(string identity);
	}
}