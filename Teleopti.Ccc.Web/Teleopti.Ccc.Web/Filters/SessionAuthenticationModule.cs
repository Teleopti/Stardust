using System.IdentityModel.Services;
using System.Web;

namespace Teleopti.Ccc.Web.Filters
{
	public class SessionAuthenticationModule : ISessionAuthenticationModule
	{
		private readonly System.IdentityModel.Services.SessionAuthenticationModule _sessionAuthenticationModule;

		public SessionAuthenticationModule()
		{
			if(HttpContext.Current==null)
				return;
			_sessionAuthenticationModule = FederatedAuthentication.SessionAuthenticationModule;
		}

		public string CookieName
		{
			get { return _sessionAuthenticationModule.CookieHandler.Name; }
		}
	}
}