namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public interface IIdentityAuthentication
	{
		ApplicationAuthenticationResult Logon(string identity);
	}
}