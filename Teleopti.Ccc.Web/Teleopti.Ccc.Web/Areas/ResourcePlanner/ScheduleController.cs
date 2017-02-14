using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ScheduleController : ApiController
	{
		private readonly FullScheduling _fullScheduling;
		private readonly IAgentGroupStaffLoader _agentGroupStaffLoader;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;

		public ScheduleController(FullScheduling fullScheduling, IAgentGroupStaffLoader agentGroupStaffLoader, IPlanningPeriodRepository planningPeriodRepository)
		{
			_fullScheduling = fullScheduling;
			_agentGroupStaffLoader = agentGroupStaffLoader;
			_planningPeriodRepository = planningPeriodRepository;
		}

		//remove me when we move scheduling/optimization out of http request
		[HttpPost, Route("api/ResourcePlanner/KeepAlive")]
		public virtual void KeepAlive()
		{
		}

		[HttpPost, Route("api/ResourcePlanner/Schedule/FixedStaff")]
		public virtual IHttpActionResult FixedStaff([FromBody] StaffSchedulingInput input)
		{
			var period = new DateOnlyPeriod(new DateOnly(input.StartDate), new DateOnly(input.EndDate));
			return Ok(_fullScheduling.DoScheduling(period));
		}

		[HttpPost, Route("api/ResourcePlanner/Schedule/{id}")]
		public virtual IHttpActionResult ScheduleForPlanningPeriod(Guid id)
		{
			var planningPeriod = _planningPeriodRepository.Load(id);
			var period = planningPeriod.Range;
			var people = _agentGroupStaffLoader.Load(period, planningPeriod.AgentGroup);
			return Ok(_fullScheduling.DoScheduling(period, people.AllPeople.Select(x => x.Id.Value)));
		}
	}
}