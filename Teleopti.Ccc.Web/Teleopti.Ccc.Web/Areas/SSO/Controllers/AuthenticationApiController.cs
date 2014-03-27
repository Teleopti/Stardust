using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;

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