using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Rta.Interfaces;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
    public class RtaController : Controller
    {
	    private readonly ITeleoptiRtaService _teleoptiRtaService;

	    public RtaController(ITeleoptiRtaService teleoptiRtaService)
	    {
		    _teleoptiRtaService = teleoptiRtaService;
	    }

	    public int SaveExternalUserState(string externalState)
		{
		//	_teleoptiRtaService.SaveExternalUserState()
		    return -1;
		}

    }
}
