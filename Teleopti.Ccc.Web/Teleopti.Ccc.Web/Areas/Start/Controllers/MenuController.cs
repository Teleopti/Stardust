using System.Web.Mvc;
using System.Web.Routing;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	using System.Linq;

	using Teleopti.Ccc.Web.Areas.Start.Core.Menu;

	public class MenuController : Controller
    {
		private readonly IMenuViewModelFactory _menuViewModelFactory;

		public MenuController(IMenuViewModelFactory menuViewModelFactory)
    	{
    		_menuViewModelFactory = menuViewModelFactory;
    	}

		public ActionResult Index()
        {
			var menuViewModels = _menuViewModelFactory.CreateMenyViewModel();
			
			if (menuViewModels.Count() == 1)
			{
				return RedirectToRoute(new RouteValueDictionary(new { area = menuViewModels.First().Area, controller = string.Empty, action = string.Empty }));
			}

			return View(menuViewModels);
        }

    	public ViewResult MobileMenu()
    	{
			return View(_menuViewModelFactory.CreateMenyViewModel());
    	}

		public ViewResult Menu()
    	{
			// until 
			//return RedirectToRoute(new RouteValueDictionary(new { area = "MyTime", controller = "", action = "" }));
			return View(_menuViewModelFactory.CreateMenyViewModel());
    	}
    }
}
