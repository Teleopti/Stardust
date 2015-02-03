namespace Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services
{
    public interface IAuthenticator
    {
        AuthenticateResult AuthenticateWindowsUser(string dataSourceName);
	    AuthenticateResult AuthenticateApplicationIdentityUser(string dataSourceName);
    }
}
