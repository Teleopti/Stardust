using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class AuthenticationController : Controller
	{
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;
		private readonly IFormsAuthentication _formsAuthentication;

		public AuthenticationController(ILayoutBaseViewModelFactory layoutBaseViewModelFactory, IFormsAuthentication formsAuthentication)
		{
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
			_formsAuthentication = formsAuthentication;
		}

		public ActionResult Index()
		{
			return RedirectToAction("", "Authentication");
		}

		public ViewResult SignIn()
		{
			ViewBag.LayoutBase = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel();
			return View();
		}

		public ActionResult SignOut()
		{
			_formsAuthentication.SignOut();
			return RedirectToAction("", "Authentication");
		}
	}

}