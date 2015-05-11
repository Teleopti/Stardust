namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IApplicationAuthentication
	{
		TenantAuthenticationResult Logon(string userName, string password);
	}
}