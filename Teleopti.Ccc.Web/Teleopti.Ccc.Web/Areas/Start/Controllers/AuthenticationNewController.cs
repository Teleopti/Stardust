using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class AuthenticationNewController : Controller
	{
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;

		public AuthenticationNewController(ILayoutBaseViewModelFactory layoutBaseViewModelFactory)
		{
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
		}

		public ViewResult Index()
		{
			return View();
		}

		public ViewResult SignIn()
		{
			ViewBag.LayoutBase = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel();
			return View();
		}

	}

}
