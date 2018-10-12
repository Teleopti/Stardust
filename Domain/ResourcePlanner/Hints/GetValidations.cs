using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class GetValidations
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IPlanningGroupStaffLoader _planningGroupStaffLoader;
		private readonly CheckScheduleHints _basicCheckScheduleHints;
		private readonly BlockPreferenceProviderUsingFiltersFactory _blockPreferenceProviderUsingFiltersFactory;

		public GetValidations(IPlanningPeriodRepository planningPeriodRepository, IPlanningGroupStaffLoader planningGroupStaffLoader, CheckScheduleHints basicCheckScheduleHints, BlockPreferenceProviderUsingFiltersFactory blockPreferenceProviderUsingFiltersFactory)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_planningGroupStaffLoader = planningGroupStaffLoader;
			_basicCheckScheduleHints = basicCheckScheduleHints;
			_blockPreferenceProviderUsingFiltersFactory = blockPreferenceProviderUsingFiltersFactory;
		}

		public HintResult Execute(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var people = _planningGroupStaffLoader.Load(planningPeriod.Range, planningPeriod.PlanningGroup).AllPeople.ToList();
			var validationResult = _basicCheckScheduleHints.Execute(
				new HintInput(null, people, planningPeriod.Range, _blockPreferenceProviderUsingFiltersFactory.Create(planningPeriod.PlanningGroup), true));
			validationResult.InvalidResources = HintsHelper.BuildBusinessRulesValidationResults(validationResult.InvalidResources);
			return validationResult;
		}
	}
}