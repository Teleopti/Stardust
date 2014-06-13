using System;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
    public class RtaServiceController : Controller
    {
	    private readonly ITeleoptiRtaService _teleoptiRtaService;

	    public RtaServiceController(ITeleoptiRtaService teleoptiRtaService)
	    {
		    _teleoptiRtaService = teleoptiRtaService;
	    }

	    public int SaveExternalUserState(string externalState)
	    {
		    var state = JsonConvert.DeserializeObject<ajaxUserState>(externalState);
		    return _teleoptiRtaService.SaveExternalUserState(state.AuthenticationKey, 
				state.UserCode, 
				state.StateCode,
			    state.StateDescription, 
				state.IsLoggedOn, 
				state.SecondsInState, 
				state.Timestamp, 
				state.PlatformTypeId,
			    state.SourceId, 
				state.BatchId, 
				state.IsSnapshot);
	    }

	    private class ajaxUserState
	    {
			public string AuthenticationKey { get; set; }

		    public string UserCode { get; set; }

		    public string StateCode { get; set; }

		    public string StateDescription { get; set; }

		    public bool IsLoggedOn { get; set; }

		    public int SecondsInState { get; set; }

		    public DateTime Timestamp { get; set; }

			public string PlatformTypeId { get; set; }

			public string SourceId { get; set; }

		    public DateTime BatchId { get; set; }

		    public bool IsSnapshot { get; set; }
	    }
    }
}
