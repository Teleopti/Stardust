using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class AuthenticationNewController : Controller
	{
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;
		private readonly IEnumerable<IDataSourcesViewModelFactory> _dataSourcesViewModelFactories;

		public AuthenticationNewController(ILayoutBaseViewModelFactory layoutBaseViewModelFactory, IEnumerable<IDataSourcesViewModelFactory> dataSourcesViewModelFactories)
		{
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
			_dataSourcesViewModelFactories = dataSourcesViewModelFactories;
		}

		public ViewResult SignIn()
		{
			ViewBag.LayoutBase = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel();
			return View();
		}

		[HttpGet]
		public JsonResult DataSources()
		{
			var sources = from f in _dataSourcesViewModelFactories
			              from s in f.DataSources()
			              select s;
			return Json(sources, JsonRequestBehavior.AllowGet);
		}

	}
}
