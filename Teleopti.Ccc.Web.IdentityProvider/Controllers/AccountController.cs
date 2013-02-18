using System.Web.Mvc;
using System.Web.Routing;
using Teleopti.Ccc.Web.IdentityProvider.Models;

namespace Teleopti.Ccc.Web.IdentityProvider.Controllers
{
	 public class AccountController : Controller
	 {

		  public IFormsAuthenticationService FormsService { get; set; }

		  protected override void Initialize(RequestContext requestContext)
		  {
				if (FormsService == null) { FormsService = new FormsAuthenticationService(); }

				base.Initialize(requestContext);
		  }

		  // **************************************
		  // URL: /Account/LogOn
		  // **************************************

		  [HttpPost]
		  public ActionResult LogOn(LogOnModel model, string returnUrl)
		  {
				FormsService.SignIn(model.UserName, model.RememberMe);
				if (Url.IsLocalUrl(returnUrl))
				{
					return Redirect(returnUrl);
				}
				return RedirectToAction("Identifier", "OpenId");
		  }

		  // **************************************
		  // URL: /Account/LogOff
		  // **************************************

		  public ActionResult LogOff()
		  {
				FormsService.SignOut();

				return RedirectToAction("Index", "Home");
		  }

	 }
}
