using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.ResourcePlanner
{
	public class BlockPreferenceProviderForPlanningPeriod : IBlockPreferenceProviderForPlanningPeriod
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;

		public BlockPreferenceProviderForPlanningPeriod(IPlanningPeriodRepository planningPeriodRepository)
		{
			_planningPeriodRepository = planningPeriodRepository;
		}
		
		public IBlockPreferenceProvider Fetch(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var planningGroup = planningPeriod.PlanningGroup;
			return new BlockPreferenceProviderUsingFilters(planningPeriod.PlanningGroup.Settings);
		}
	}
}