using System.Web;
using Microsoft.IdentityModel.Web;

namespace Teleopti.Ccc.Web.Filters
{
	public class AuthenticationModule: IAuthenticationModule
	{
		private readonly WSFederationAuthenticationModule _authenticationModule;

		public AuthenticationModule()
		{
			if(HttpContext.Current==null)
				return;
			_authenticationModule = FederatedAuthentication.WSFederationAuthenticationModule;
		}
		public string Issuer
		{
			get { return _authenticationModule.Issuer; }
		}
		public string Realm
		{
			get { return _authenticationModule.Realm; }
		}
	}
}