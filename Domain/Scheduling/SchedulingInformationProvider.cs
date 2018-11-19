using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingInformationProvider
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IPlanningGroupStaffLoader _planningGroupStaffLoader;

		public SchedulingInformationProvider(IPlanningPeriodRepository planningPeriodRepository, IPlanningGroupStaffLoader planningGroupStaffLoader)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_planningGroupStaffLoader = planningGroupStaffLoader;
		}

		[TestLog]
		[UnitOfWork]
		public virtual SchedulingInformation GetInfoFromPlanningPeriod(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;
			var people = _planningGroupStaffLoader.Load(period, planningPeriod.PlanningGroup).AllPeople.Select(x => x.Id.Value).ToList();
			return new SchedulingInformation(period, people, planningPeriod.PlanningGroup.Id.Value);
		}
	}

	public class SchedulingInformation
	{
		public IList<Guid> PersonIds { get; }
		public Guid PlanningGroupId { get; }
		public DateOnlyPeriod Period { get; }

		public SchedulingInformation(DateOnlyPeriod period, IList<Guid> personIds, Guid planningGroupId)
		{
			PersonIds = personIds;
			PlanningGroupId = planningGroupId;
			Period = period;
		}
	}
}