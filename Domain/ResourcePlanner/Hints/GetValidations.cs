using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class GetValidations
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IPlanningGroupStaffLoader _planningGroupStaffLoader;
		private readonly CheckScheduleHints _basicCheckScheduleHints;

		public GetValidations(IPlanningPeriodRepository planningPeriodRepository, IPlanningGroupStaffLoader planningGroupStaffLoader, CheckScheduleHints basicCheckScheduleHints)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_planningGroupStaffLoader = planningGroupStaffLoader;
			_basicCheckScheduleHints = basicCheckScheduleHints;
		}

		public HintResult Execute(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var people = _planningGroupStaffLoader.Load(planningPeriod.Range, planningPeriod.PlanningGroup).AllPeople.ToList();
			var validationResult = _basicCheckScheduleHints.Execute(
				new ScheduleHintInput(people, planningPeriod.Range, true));
			validationResult.InvalidResources = HintsHelper.BuildBusinessRulesValidationResults(validationResult.InvalidResources);
			return validationResult;
		}
	}
}