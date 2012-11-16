using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class AuthenticationNewController : Controller
	{
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;
		private readonly IEnumerable<IDataSourcesViewModelFactory> _dataSourcesViewModelFactories;
		private readonly IBusinessUnitsViewModelFactory _businessUnitViewModelFactory;

		public AuthenticationNewController(ILayoutBaseViewModelFactory layoutBaseViewModelFactory, IEnumerable<IDataSourcesViewModelFactory> dataSourcesViewModelFactories, IBusinessUnitsViewModelFactory businessUnitViewModelFactory)
		{
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
			_dataSourcesViewModelFactories = dataSourcesViewModelFactories;
			_businessUnitViewModelFactory = businessUnitViewModelFactory;
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

		[HttpGet]
		public JsonResult BusinessUnits(IAuthenticationModel model)
		{
			var result = model.AuthenticateUser();
			var businessUnits = _businessUnitViewModelFactory.BusinessUnits(result.DataSource, result.Person);
			return Json(businessUnits, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Logon(IAuthenticationModel model, Guid businessUnitId)
		{
			var result = model.AuthenticateUser();
			// .. do even more stuff and setup session etc
			return Json(new object());
		}
	}


}
