﻿using System;
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

		    return Json(_teleoptiRtaService.SaveExternalUserState(state.AuthenticationKey, 
				state.UserCode, 
				state.StateCode,
			    state.StateDescription, 
				state.IsLoggedOn, 
				state.SecondsInState, 
				timestamp, 
				state.PlatformTypeId,
			    state.SourceId, 
				state.BatchId,
				state.IsSnapshot));
	    }
    }
}
