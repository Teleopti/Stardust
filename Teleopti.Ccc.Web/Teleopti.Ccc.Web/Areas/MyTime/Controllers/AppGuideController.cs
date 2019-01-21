using System.Web.Mvc;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.MyTime.Models.AppGuide;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.ViewQRCodeForConfiguration)]
	public class AppGuideController : Controller
	{
		private readonly ICurrentTenant _currentTenant;

		public AppGuideController(ICurrentTenant currentTenant)
		{
			_currentTenant = currentTenant;
		}

		[HttpGet]
		[TenantUnitOfWork]
		public virtual PartialViewResult WFMApp()
		{
			var url = _currentTenant.Current().GetApplicationConfig(TenantApplicationConfigKey.MobileQRCodeUrl) ?? string.Empty;
			return PartialView("WFMAppPartial", new WFMAppGuideViewModel
			{
				UrlForMyTimeWeb = url
			});
		}
	}
}