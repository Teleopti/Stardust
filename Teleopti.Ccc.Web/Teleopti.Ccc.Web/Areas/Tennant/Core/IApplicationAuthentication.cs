namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public interface IApplicationAuthentication
	{
		ApplicationAuthenticationResult Logon(string userName, string password);
	}
}