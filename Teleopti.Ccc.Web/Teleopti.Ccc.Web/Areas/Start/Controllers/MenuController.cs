using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.Start.Core.Menu;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class MenuController : Controller
    {
		private readonly IMenuViewModelFactory _menuViewModelFactory;

		public MenuController(IMenuViewModelFactory menuViewModelFactory)
    	{
    		_menuViewModelFactory = menuViewModelFactory;
    	}

		[HttpGet]
		public JsonResult Applications()
		{
			return Json(_menuViewModelFactory.CreateMenuViewModel(), JsonRequestBehavior.AllowGet);
		}
    }
}
