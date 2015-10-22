using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
    public class IntradaySkillStatusController : ApiController
    {
	    private readonly ISkillTasksDetailProvider _skillTasksDetailProvider;

	    public IntradaySkillStatusController(ISkillTasksDetailProvider skillTasksDetailProvider)
	    {
		    _skillTasksDetailProvider = skillTasksDetailProvider;
	    }

	    [HttpGet]
	    [UnitOfWork]
	    [Route("GetSkillStatus")]
	    public virtual JsonResult<List<SkillStatusModel>> GetSkillStatus()
	    {
		    var ret = new List<SkillStatusModel>();
          var taskDetails = _skillTasksDetailProvider.GetForecastedTasks();

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

	public class SkillStatusModel
	{
		public string	SkillName { get; set; }
		public int  Severity { get; set; }

		public List<SkillStatusMeasure> Measures { get; set; }
	}

	public class SkillStatusMeasure
	{
		public string Name { get; set; }
		public double Value { get; set; }
		public string StringValue { get; set; }
		public int Severity { get; set; }
	}
}
