using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
    public class IntradaySkillStatusController : ApiController
    {
	    private readonly IIntradaySkillStatusService _intradaySkillStatusService;

		 public IntradaySkillStatusController(IIntradaySkillStatusService intradaySkillStatusService)
		 {
			 _intradaySkillStatusService = intradaySkillStatusService;
		 }

		 [UnitOfWork, HttpGet, Route("api/intraday/skillstatus"), AuthorizeTeleopti]
		 public virtual IHttpActionResult GetSkillStatus()
	    {
			 return Ok(_intradaySkillStatusService.GetSkillStatusModels(DateTime.UtcNow));
	    }
    }
}
