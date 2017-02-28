using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class SchedulePlanningPeriodCommandHandler : ISchedulePlanningPeriodCommandHandler
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly FullScheduling _fullScheduling;
		private readonly IAgentGroupStaffLoader _agentGroupStaffLoader;

		public SchedulePlanningPeriodCommandHandler(IPlanningPeriodRepository planningPeriodRepository, FullScheduling fullScheduling, IAgentGroupStaffLoader agentGroupStaffLoader)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_fullScheduling = fullScheduling;
			_agentGroupStaffLoader = agentGroupStaffLoader;
		}

		public object Execute(SchedulePlanningPeriodCommand command)
		{
			var schedulingData = GetInfoFromPlanningPeriod(command.PlanningPeriodId);
			return _fullScheduling.DoScheduling(schedulingData.Item1, schedulingData.Item2);
		}

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