using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroupProvider : IPlanningGroupProvider
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;

		public PlanningGroupProvider(IPlanningPeriodRepository planningPeriodRepository)
		{
			_planningPeriodRepository = planningPeriodRepository;
		}

		public PlanningGroup Execute(Guid planningPeriodId)
		{
			return _planningPeriodRepository.Get(planningPeriodId).PlanningGroup;
		}
	}
}