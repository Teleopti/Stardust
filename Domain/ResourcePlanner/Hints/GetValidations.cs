using System;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class GetValidations
	{
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IPlanningGroupStaffLoader _planningGroupStaffLoader;
		private readonly CheckScheduleHints _basicCheckScheduleHints;
		private readonly IUserTimeZone _userTimeZone;

		public GetValidations(IPlanningPeriodRepository planningPeriodRepository, IPlanningGroupStaffLoader planningGroupStaffLoader, CheckScheduleHints basicCheckScheduleHints, IUserTimeZone userTimeZone)
		{
			_planningPeriodRepository = planningPeriodRepository;
			_planningGroupStaffLoader = planningGroupStaffLoader;
			_basicCheckScheduleHints = basicCheckScheduleHints;
			_userTimeZone = userTimeZone;
		}

		public HintResult Execute(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var people = _planningGroupStaffLoader.Load(planningPeriod.Range, planningPeriod.PlanningGroup).AllPeople.ToList();
			var validationResult = _basicCheckScheduleHints.Execute(
				new ScheduleHintInput(people, planningPeriod.Range, planningPeriod.PlanningGroup.Settings.PreferenceValue.Value));
			validationResult.InvalidResources = HintsHelper.BuildBusinessRulesValidationResults(validationResult.InvalidResources, _userTimeZone);
			return validationResult;
		}
	}
}