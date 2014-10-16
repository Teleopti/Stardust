using System;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Rta.WebService;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
    public class ServiceController : Controller
    {
		private readonly TeleoptiRtaService _teleoptiRtaService;

	    public ServiceController(TeleoptiRtaService teleoptiRtaService)
	    {
		    _teleoptiRtaService = teleoptiRtaService;
	    }

		[HttpPost]
		public JsonResult SaveExternalUserState(ExternalUserStateWebModel state)
		{
			var result = _teleoptiRtaService.SaveExternalUserState(
				new ExternalUserStateInputModel
				{
					AuthenticationKey = state.AuthenticationKey,
					PlatformTypeId = state.PlatformTypeId,
					SourceId = state.SourceId,
					UserCode = state.UserCode,
					StateCode = state.StateCode,
					StateDescription = state.StateDescription,
					IsLoggedOn = state.IsLoggedOn,
					SecondsInState = state.SecondsInState,
					Timestamp = DateTime.Parse(state.Timestamp),
					BatchId = DateTime.Parse(state.BatchId),
					IsSnapshot = state.IsSnapshot,
				});

			// apparently 1 = state accepted, 0 = something was missing, anything else == error
			if (result == 1 || result == 0)
				return Json(result);
			throw new HttpException("Result from TeleoptiRtaService was " + result);
	    }
    }
}
