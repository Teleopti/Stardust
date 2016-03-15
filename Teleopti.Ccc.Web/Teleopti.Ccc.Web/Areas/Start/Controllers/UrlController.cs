using System;
using System.Configuration;
using System.Web.Mvc;
using Microsoft.IdentityModel.Protocols.WSFederation;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Config;
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

	    public UrlController(ICurrentHttpContext currentHttpContext, IAuthenticationModule authenticationModule, IIdentityLogon identityLogon, SignatureCreator signatureCreator)
        {
	        _currentHttpContext = currentHttpContext;
	        _authenticationModule = authenticationModule;
            _identityLogon = identityLogon;
		    _signatureCreator = signatureCreator;
        }

	    public ActionResult Index()
        {
            var currentHttp = _currentHttpContext.Current();
			var url = new Uri(currentHttp.Request.Url, currentHttp.Response.ApplyAppPathModifier("~/")).ToString();
            var result = _signatureCreator.Create(url);

            return Json(new { Url = url, Signature = result }, JsonRequestBehavior.AllowGet);
        }

		  public ActionResult RedirectToWebLogin()
		  {
			  string issuer = _authenticationModule.Issuer(_currentHttpContext.Current()).ToString();

			  var signIn = new SignInRequestMessage(new Uri(issuer), _authenticationModule.Realm)
			  {
				  Context = "ru=" + _currentHttpContext.Current().Request.Url.AbsoluteUri.Replace("start/Url/RedirectToWebLogin", ""),
				  HomeRealm = "urn:"
			  };

			  var url = signIn.WriteQueryString();
			  var uri = new Uri(url, UriKind.RelativeOrAbsolute);
			  var redirectUrl = ConfigurationManager.AppSettings.ReadValue("UseRelativeConfiguration")
				  ? "/" + new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped)).MakeRelativeUri(uri)
				  : url;
			  return Redirect(redirectUrl);
		  }

          [HttpGet]
          [TenantUnitOfWork]
          [NoTenantAuthentication]
          public virtual JsonResult AuthenticationDetails()
		 {
		     var loggedOnPerson = _identityLogon.LogonIdentityUser().Person;
			 return Json(new { PersonId = loggedOnPerson.Id }, JsonRequestBehavior.AllowGet);
	    }
    }
}