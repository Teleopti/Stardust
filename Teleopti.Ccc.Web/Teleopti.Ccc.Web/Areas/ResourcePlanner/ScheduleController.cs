using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
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

		[HttpPost, Route("api/ResourcePlanner/Schedule/{id}")]
		public virtual IHttpActionResult ScheduleForPlanningPeriod(Guid id)
		{
			var schedulingData = GetInfoFromPlanningPeriod(id);
			return Ok(_fullScheduling.DoScheduling(schedulingData.Item1, schedulingData.Item2));
		}

		// Temporary until we move to stardust
		[UnitOfWork]
		protected virtual Tuple<DateOnlyPeriod, IList<Guid>> GetInfoFromPlanningPeriod(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;
			var people = _agentGroupStaffLoader.Load(period, planningPeriod.AgentGroup);
			return new Tuple<DateOnlyPeriod, IList<Guid>>(period, people.AllPeople.Select(x => x.Id.Value).ToList());
		}

	}
}