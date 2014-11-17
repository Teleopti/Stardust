using System;
using System.Web;
using System.Web.Mvc;
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

		public Uri Issuer(HttpContextBase request)
		{
			var helper = new UrlHelper(request.Request.RequestContext);
			Uri issuerUrl;
			if (!Uri.TryCreate(_authenticationModule.Issuer,UriKind.Absolute,out issuerUrl))
			{
				issuerUrl = new Uri(new Uri(new Uri(request.Request.Url.GetLeftPart(UriPartial.Authority)), helper.Content("~")), _authenticationModule.Issuer);
			}
			return issuerUrl;
		}

		public string Realm
		{
			get { return _authenticationModule.Realm; }
		}
	}
}