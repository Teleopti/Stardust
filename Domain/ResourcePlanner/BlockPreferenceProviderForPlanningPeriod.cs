using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.ResourcePlanner
{
	public class BlockPreferenceProviderForPlanningPeriod : IBlockPreferenceProviderForPlanningPeriod
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly BlockPreferenceProviderUsingFiltersFactory _blockPreferenceProviderUsingFiltersFactory;

		public BlockPreferenceProviderForPlanningPeriod(IPlanningPeriodRepository planningPeriodRepository,
			BlockPreferenceProviderUsingFiltersFactory blockPreferenceProviderUsingFiltersFactory)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_blockPreferenceProviderUsingFiltersFactory = blockPreferenceProviderUsingFiltersFactory;
		}
		
		public IBlockPreferenceProvider Fetch(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var planningGroup = planningPeriod.PlanningGroup;
			return planningGroup == null ? 
				_blockPreferenceProviderUsingFiltersFactory.Create() : 
				_blockPreferenceProviderUsingFiltersFactory.Create(planningGroup);
		}
	}
}