using System.Web.Mvc;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.MyTime.Models;
using Teleopti.Ccc.Web.Areas.MyTime.Models.AppGuide;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class AppGuideController : Controller
	{
		private readonly ICurrentTenant _currentTenant;

		public AppGuideController(ICurrentTenant currentTenant)
		{
			_currentTenant = currentTenant;
		}

		[TenantUnitOfWork]
		public virtual PartialViewResult WFMApp()
		{
			var url = _currentTenant.Current().GetApplicationConfig(TenantApplicationConfigKey.MobileQRCodeUrl);
			return PartialView("WFMAppPartial", new WFMAppGuideViewModel
			{
				UrlForMyTimeWeb = url
			});
		}
	}
}