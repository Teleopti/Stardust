namespace Teleopti.Ccc.Web.Areas.Tenant.Core
{
	public interface IApplicationAuthentication
	{
		ApplicationAuthenticationResult Logon(string userName, string password);
	}
}