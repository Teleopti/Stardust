using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
    public class IntradaySkillStatusController : ApiController
    {
	    private readonly IIntradaySkillStatusService _intradaySkillStatusService;

		 public IntradaySkillStatusController(IIntradaySkillStatusService intradaySkillStatusService)
		 {
			 _intradaySkillStatusService = intradaySkillStatusService;
		 }

		 [UnitOfWork, HttpGet, Route("api/intraday/skillstatus"), Authorize]
		 public virtual IHttpActionResult GetSkillStatus()
	    {
		    return Ok(_intradaySkillStatusService.GetSkillStatusModels());
	    }
    }
}
