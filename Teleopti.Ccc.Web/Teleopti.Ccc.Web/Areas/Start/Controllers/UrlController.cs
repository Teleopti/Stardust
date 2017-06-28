using System;
using System.Configuration;
using System.IdentityModel.Services;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class UrlController : Controller
	{
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly IAuthenticationModule _authenticationModule;
		private readonly IIdentityLogon _identityLogon;
		private readonly SignatureCreator _signatureCreator;
		private readonly INow _now;


		public UrlController(ICurrentHttpContext currentHttpContext, IAuthenticationModule authenticationModule,
			IIdentityLogon identityLogon, SignatureCreator signatureCreator, INow now)
		{
			_currentHttpContext = currentHttpContext;
			_authenticationModule = authenticationModule;
			_identityLogon = identityLogon;
			_signatureCreator = signatureCreator;
			_now = now;
		}

		public ActionResult Index()
		{
			var currentHttp = _currentHttpContext.Current();
			var url = new Uri(currentHttp.Request.Url, currentHttp.Response.ApplyAppPathModifier("~/")).ToString();
			var result = _signatureCreator.Create(url);

			return Json(new {Url = url, Signature = result}, JsonRequestBehavior.AllowGet);
		}

		public ActionResult RedirectToWebLogin(string queryString)
		{
			string issuer = _authenticationModule.Issuer(_currentHttpContext.Current()).ToString();

			var signIn = new SignInRequestMessage(new Uri(issuer), _authenticationModule.Realm)
			{
				Context =
					"ru=" + _currentHttpContext.Current().Request.Url.AbsoluteUri.Replace("start/Url/RedirectToWebLogin", ""),
				HomeRealm = "urn:"
			};

			var url = signIn.WriteQueryString();
			var uri = new Uri(url, UriKind.RelativeOrAbsolute);
			var redirectUrl = ConfigurationManager.AppSettings.ReadValue("UseRelativeConfiguration")
				? "/" + new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)).MakeRelativeUri(uri)
				: url;
			if (!string.IsNullOrEmpty(queryString))
			{
				queryString = HttpUtility.UrlDecode(queryString);
				var httpCookieCollection = _currentHttpContext.Current().Response.Cookies;
				var parameters = queryString.Split('&');
				foreach (var parameter in parameters)
				{
					var tuple = parameter.Split('=');
					var cookie = new HttpCookie(HttpUtility.UrlDecode(tuple[0]))
					{
						Value = HttpUtility.UrlDecode(tuple[1]),
						Expires = _now.ServerDateTime_DontUse().AddDays(1),
						HttpOnly = true
					};
					httpCookieCollection.Add(cookie);
				}
			}
			return Redirect(redirectUrl);
		}

		[HttpGet]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual JsonResult AuthenticationDetails()
		{
			var loggedOnPerson = _identityLogon.LogonIdentityUser().Person;
			return Json(new {PersonId = loggedOnPerson.Id}, JsonRequestBehavior.AllowGet);
		}
	}
}