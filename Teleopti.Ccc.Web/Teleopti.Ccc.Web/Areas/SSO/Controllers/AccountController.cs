﻿using System.Web.Mvc;
using System.Web.Routing;
using Teleopti.Ccc.Web.Areas.SSO.Models;

namespace Teleopti.Ccc.Web.Areas.SSO.Controllers
{
	 public class AccountController : Controller
	 {

		  public IFormsAuthenticationService FormsService { get; set; }
		  public IMembershipService MembershipService { get; set; }

		  protected override void Initialize(RequestContext requestContext)
		  {
				if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
				if (MembershipService == null) { MembershipService = new AccountMembershipService(); }

				base.Initialize(requestContext);
		  }

		  // **************************************
		  // URL: /Account/LogOn
		  // **************************************

		  public ActionResult LogOn()
		  {
				return View();
		  }

		  [HttpPost]
		  public ActionResult LogOn(LogOnModel model, string returnUrl)
		  {
					if (MembershipService.ValidateUser(model.UserName, model.Password))
					 {
						  FormsService.SignIn(model.UserName, model.RememberMe);
						  if (Url.IsLocalUrl(returnUrl))
						  {
								return Redirect(returnUrl);
						  }
						 return RedirectToAction("Identifier", "OpenId");
					 }
					ModelState.AddModelError("", "The user name or password provided is incorrect.");

			  // If we got this far, something failed, redisplay form
				return View(model);
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
