using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.Start.Core.Config;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class ConfigController : Controller
	{
		private readonly ISharedSettingsFactory _sharedSettingsFactory;

		public ConfigController(ISharedSettingsFactory sharedSettingsFactory)
		{
			_sharedSettingsFactory = sharedSettingsFactory;
		}

		[HttpGet]
		public virtual JsonResult SharedSettings()
		{
			return Json(_sharedSettingsFactory.Create(), JsonRequestBehavior.AllowGet);
		}
	}
}