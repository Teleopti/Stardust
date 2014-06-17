using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
    public class RtaServiceController : Controller
    {
	    private readonly ITeleoptiRtaService _teleoptiRtaService;

	    public RtaServiceController(ITeleoptiRtaService teleoptiRtaService)
	    {
		    _teleoptiRtaService = teleoptiRtaService;
	    }

		[HttpPost]
		public int SaveExternalUserState(AjaxUserState state)
		{
			var timestamp = DateTime.Parse(state.Timestamp);

		    return _teleoptiRtaService.SaveExternalUserState(state.AuthenticationKey, 
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
	    }
    }
}
