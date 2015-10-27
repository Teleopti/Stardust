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

		 [HttpGet, Route("api/intraday/skillstatus"), Authorize, UnitOfWork]
	    public virtual JsonResult<IEnumerable<SkillStatusModel>> GetSkillStatus()
	    {
		    return Json(_intradaySkillStatusService.GetSkillStatusModels());
	    }
    }
}
