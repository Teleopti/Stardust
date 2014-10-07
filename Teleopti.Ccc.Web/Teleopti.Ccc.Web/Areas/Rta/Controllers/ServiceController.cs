using System;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Rta.WebService;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
    public class ServiceController : Controller
    {
	    private readonly ITeleoptiRtaService _teleoptiRtaService;

	    public ServiceController(ITeleoptiRtaService teleoptiRtaService)
	    {
		    _teleoptiRtaService = teleoptiRtaService;
	    }

		[HttpPost]
		public JsonResult SaveExternalUserState(AjaxUserState state)
		{
			var timestamp = DateTime.Parse(state.Timestamp);

			var result = _teleoptiRtaService.SaveExternalUserState(state.AuthenticationKey,
				state.UserCode,
				state.StateCode,
				state.StateDescription,
				state.IsLoggedOn,
				state.SecondsInState,
				timestamp,
				state.PlatformTypeId,
				state.SourceId,
				state.BatchId,
				state.IsSnapshot);

			// apparently 1 = state accepted, 0 = something was missing, anything else == error
			if (result == 1 || result == 0)
				return Json(result);
			throw new HttpException("Result from TeleoptiRtaService was " + result);
	    }
    }
}
