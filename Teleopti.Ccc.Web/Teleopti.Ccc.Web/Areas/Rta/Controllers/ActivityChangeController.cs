using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class ActivityChangeController : Controller
	{
		private readonly TeleoptiRtaService _teleoptiRtaService;

		public ActivityChangeController(TeleoptiRtaService teleoptiRtaService)
		{
			_teleoptiRtaService = teleoptiRtaService;
		}

		[HttpPost]
		public void CheckFor(CheckForActivityChangeInputModel model)
		{
			_teleoptiRtaService.CheckForActivityChange(model);
		}
	}
}
