using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.SSO.Core;

namespace Teleopti.Ccc.Web.Areas.SSO.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class AuthenticationApiController : Controller
	{
		private readonly IApplicationDataSourcesViewModelFactory _applicationDataSourcesViewModelFactory;

		public AuthenticationApiController(IApplicationDataSourcesViewModelFactory applicationDataSourcesViewModelFactory)
		{
			_applicationDataSourcesViewModelFactory = applicationDataSourcesViewModelFactory;
		}

		[HttpGet]
		public JsonResult DataSources()
		{
			return Json(_applicationDataSourcesViewModelFactory.DataSources(), JsonRequestBehavior.AllowGet);
		}
	}
}