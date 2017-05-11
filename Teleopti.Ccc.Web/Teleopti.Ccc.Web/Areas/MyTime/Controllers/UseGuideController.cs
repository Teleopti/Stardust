using System.Web.Mvc;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Web.Areas.MyTime.Models.UseGuide;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class UseGuideController : Controller
	{
		private readonly ICurrentTenant _currentTenant;

		public UseGuideController(ICurrentTenant currentTenant)
		{
			_currentTenant = currentTenant;
		}

		[TenantUnitOfWork]
		public PartialViewResult WFMApp()
		{
			var url = _currentTenant.Current().GetApplicationConfig(TenantApplicationConfigKey.MobileQRCodeUrl);
			return PartialView("WFMAppPartial", new WFMAppGuideViewModel
			{
				UrlForMyTimeWeb = url
			});
		}
	}
}