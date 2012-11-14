using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Shared;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class AuthenticationNewController : Controller
	{
		private readonly ILayoutBaseViewModelFactory _layoutBaseViewModelFactory;
		private readonly IDataSourcesProvider _dataSourceProvider;

		public AuthenticationNewController(ILayoutBaseViewModelFactory layoutBaseViewModelFactory, IDataSourcesProvider dataSourceProvider)
		{
			_layoutBaseViewModelFactory = layoutBaseViewModelFactory;
			_dataSourceProvider = dataSourceProvider;
		}

		public ActionResult SignInNew()
		{
			ViewBag.LayoutBase = _layoutBaseViewModelFactory.CreateLayoutBaseViewModel();
			return View();
		}

		[HttpGet]
		public JsonResult LoadDataSources()
		{
			var applicatoinDataSources = _dataSourceProvider.RetrieveDatasourcesForApplication()
				.SelectOrEmpty(
					x => new DataSourceViewModel { Name = x.DataSourceName, DisplayName = x.DataSourceName, IsApplicationLogon = true });
			var windwowsDataSources = _dataSourceProvider.RetrieveDatasourcesForWindows()
				.SelectOrEmpty(
					x => new DataSourceViewModel { Name = x.DataSourceName, DisplayName = x.DataSourceName + " " + Resources.WindowsLogonWithBrackets, IsApplicationLogon = false });
			return Json(applicatoinDataSources.Union(windwowsDataSources), JsonRequestBehavior.AllowGet);
		}

	}
}
