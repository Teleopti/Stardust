namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IApplicationAuthentication
	{
		ApplicationAuthenticationResult Logon(string userName, string password);
	}
}