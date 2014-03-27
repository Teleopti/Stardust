using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;

namespace Teleopti.Ccc.Web.Areas.SSO.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class AuthenticationController : Controller
	{
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;

		public AuthenticationController(ILayoutBaseViewModelFactory layoutBaseViewModelFactory)
		{
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
		}

		public ViewResult SignIn(string returnUrl)
		{
			ViewBag.LayoutBase = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel();
			ViewBag.ReturnUrl = returnUrl;
			return View();
		}
	}
}