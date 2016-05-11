using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class UrlController : Controller
	{
		private readonly ICurrentHttpContext _currentHttpContext;
		private readonly IIdentityLogon _identityLogon;
		private readonly SignatureCreator _signatureCreator;

		public UrlController(ICurrentHttpContext currentHttpContext, IIdentityLogon identityLogon,
			SignatureCreator signatureCreator)
		{
			_currentHttpContext = currentHttpContext;
			_identityLogon = identityLogon;
			_signatureCreator = signatureCreator;
		}

		public ActionResult Index()
		{
			var currentHttp = _currentHttpContext.Current();
			var url = new Uri(currentHttp.Request.Url, currentHttp.Response.ApplyAppPathModifier("~/")).ToString();
			var result = _signatureCreator.Create(url);

			return Json(new {Url = url, Signature = result}, JsonRequestBehavior.AllowGet);
		}

		public ActionResult RedirectToWebLogin()
		{
			return Redirect("~/logout");
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