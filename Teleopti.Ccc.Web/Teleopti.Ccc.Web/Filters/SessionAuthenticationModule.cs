using System.Web;
using Microsoft.IdentityModel.Web;

namespace Teleopti.Ccc.Web.Filters
{
	public class SessionAuthenticationModule : ISessionAuthenticationModule
	{
		private readonly Microsoft.IdentityModel.Web.SessionAuthenticationModule _sessionAuthenticationModule;

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