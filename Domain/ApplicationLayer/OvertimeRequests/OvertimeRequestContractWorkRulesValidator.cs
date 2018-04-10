using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests
{
	public class OvertimeRequestContractWorkRulesValidator : IOvertimeRequestContractWorkRulesValidator
	{
		private readonly ICurrentScenario _scenarioRepository;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly ILoadSchedulesForRequestWithoutResourceCalculation
			_loadSchedulingDataForRequestWithoutResourceCalculation;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

		public OvertimeRequestContractWorkRulesValidator(ICurrentScenario scenarioRepository,
			ISchedulingResultStateHolder schedulingResultStateHolder,
			ILoadSchedulesForRequestWithoutResourceCalculation loadSchedulingDataForRequestWithoutResourceCalculation,
			IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_scenarioRepository = scenarioRepository;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_loadSchedulingDataForRequestWithoutResourceCalculation = loadSchedulingDataForRequestWithoutResourceCalculation;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public OvertimeRequestValidationResult Validate(IPersonRequest personRequest, OvertimeRequestSkillTypeFlatOpenPeriod overtimeRequestOpenPeriod)
		{
			var person = personRequest.Person;
			var timeZone = person.PermissionInformation.DefaultTimeZone();

			if (!overtimeRequestOpenPeriod.EnableWorkRuleValidation)
				return new OvertimeRequestValidationResult { IsValid = true };

			loadSchedules(personRequest.Request.Period, person, _scenarioRepository.Current());

			var scheduleDictionary = _schedulingResultStateHolder.Schedules;

			var undoRedoContainer = new UndoRedoContainer();
			setupUndo(undoRedoContainer, _schedulingResultStateHolder);

			var personLocalDate = new DateOnly(personRequest.Request.Period.StartDateTimeLocal(timeZone));
			var scheduleDay = scheduleDictionary[person].ScheduledDay(personLocalDate);
			scheduleDay.CreateAndAddOvertime(new Scheduling.Activity("fake") { InWorkTime = true }, personRequest.Request.Period, ((IOvertimeRequest)personRequest.Request).MultiplicatorDefinitionSet, true);

			var repsonses = scheduleDictionary.Modify(ScheduleModifier.Scheduler, scheduleDay,
				NewBusinessRuleCollection.WorkRules(),
				_scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance)).ToList();

			undoRedoContainer.UndoAll();

			if (repsonses.Any())
			{
				return new OvertimeRequestValidationResult
				{
					InvalidReasons = repsonses.Select(x => x.Message).Distinct().ToArray(),
					IsValid = false,
					ShouldDenyIfInValid = overtimeRequestOpenPeriod.WorkRuleValidationHandleType ==
						OvertimeValidationHandleType.Deny,
					BrokenBusinessRules = NewBusinessRuleCollection.GetFlagFromRules(repsonses.Select(x=>x.TypeOfRule))
				};
			}

			return new OvertimeRequestValidationResult
			{
				IsValid = true
			};
		}

		private void loadSchedules(DateTimePeriod period, IPerson person, IScenario scenario)
		{
			_loadSchedulingDataForRequestWithoutResourceCalculation.Execute(scenario, period, new List<IPerson> { person }, _schedulingResultStateHolder);
		}

		private static void setupUndo(IUndoRedoContainer undoRedoContainer,
			ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			if (schedulingResultStateHolder.Schedules != null)
			{
				schedulingResultStateHolder.Schedules.TakeSnapshot();
				schedulingResultStateHolder.Schedules.SetUndoRedoContainer(undoRedoContainer);
			}
		}
	}
}