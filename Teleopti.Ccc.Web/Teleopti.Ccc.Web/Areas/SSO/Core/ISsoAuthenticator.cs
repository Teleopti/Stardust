using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Areas.SSO.Core
{
	public interface ISsoAuthenticator
	{
		AuthenticateResult AuthenticateApplicationUser(string dataSourceName, string userName, string password);
	}
}