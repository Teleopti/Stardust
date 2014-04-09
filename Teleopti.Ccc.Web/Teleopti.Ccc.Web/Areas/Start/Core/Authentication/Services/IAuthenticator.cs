namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
    public interface IAuthenticator
    {
        AuthenticateResult AuthenticateWindowsUser(string dataSourceName);
		AuthenticateResult AuthenticateApplicationUser(string dataSourceName, string userName, string password);
		void SaveAuthenticateResult(string userName, AuthenticateResult result);
	    AuthenticateResult AuthenticateApplicationIdentityUser(string dataSourceName);
    }
}
