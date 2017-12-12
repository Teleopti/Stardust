using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.ResourcePlanner
{
	public class DayOffOptimizationPreferenceProviderForPlanningPeriod : IDayOffOptimizationPreferenceProviderForPlanningPeriod
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly DayOffOptimizationPreferenceProviderUsingFiltersFactory _dayOffOptimizationPreferenceProviderUsingFiltersFactory;

		public DayOffOptimizationPreferenceProviderForPlanningPeriod(IPlanningPeriodRepository planningPeriodRepository,
			DayOffOptimizationPreferenceProviderUsingFiltersFactory dayOffOptimizationPreferenceProviderUsingFiltersFactory)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_dayOffOptimizationPreferenceProviderUsingFiltersFactory = dayOffOptimizationPreferenceProviderUsingFiltersFactory;
		}
		
		public IDayOffOptimizationPreferenceProvider Fetch(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var planningGroup = planningPeriod.PlanningGroup;
			return planningGroup == null ? 
				_dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create() : 
				_dayOffOptimizationPreferenceProviderUsingFiltersFactory.Create(planningGroup);
		}
	}
}