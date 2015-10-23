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
	    private readonly IntradaySkillStatusService _intradaySkillStatusService;

		 public IntradaySkillStatusController(IntradaySkillStatusService intradaySkillStatusService)
		 {
			 _intradaySkillStatusService = intradaySkillStatusService;
		 }

	    [HttpGet]
	    [UnitOfWork]
	    [Route("GetSkillStatus")]
	    public virtual JsonResult<List<SkillStatusModel>> GetSkillStatus()
	    {
		    var ret = new List<SkillStatusModel>();
			 var taskDetails = _intradaySkillStatusService.GetForecastedTasks();

		    foreach (KeyValuePair<ISkill, IList<SkillTaskDetails>> pair in taskDetails)
		    {
			    var skill = pair.Key;
			    var values = pair.Value;
			    var sumvalues = values.Sum(tasks => tasks.Task);
			    ret.Add(new SkillStatusModel {SkillName = skill.Name, Measures = new List<SkillStatusMeasure> {new SkillStatusMeasure{Name = "Calls", Value = sumvalues,Severity = 1}} });
		    }
		    return Json(ret);
	    }
    }
}
