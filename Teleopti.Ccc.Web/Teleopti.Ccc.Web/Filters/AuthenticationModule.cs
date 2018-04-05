using System;
using System.IdentityModel.Services;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Auth;

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

		public Uri Issuer(HttpContextBase request)
		{
			var helper = new UrlHelper(request.Request.RequestContext);
			if (!Uri.TryCreate(_authenticationModule.Issuer,UriKind.Absolute,out var issuerUrl))
			{
				issuerUrl = new Uri(new Uri(request.Request.UrlConsideringLoadBalancerHeaders().GetLeftPart(UriPartial.Authority)), _authenticationModule.Issuer);
			}
			return issuerUrl;
		}

		public string Realm => _authenticationModule.Realm;
	}
}