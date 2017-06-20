using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class FullSchedulingResult
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly ViolatedSchedulePeriodBusinessRule _violatedSchedulePeriodBusinessRule;
		private readonly DayOffBusinessRuleValidation _dayOffBusinessRuleValidation;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public FullSchedulingResult(Func<ISchedulerStateHolder> schedulerStateHolder,
			ViolatedSchedulePeriodBusinessRule violatedSchedulePeriodBusinessRule,
			DayOffBusinessRuleValidation dayOffBusinessRuleValidation, ICurrentUnitOfWork currentUnitOfWork)
		{
			_schedulerStateHolder = schedulerStateHolder;
			_violatedSchedulePeriodBusinessRule = violatedSchedulePeriodBusinessRule;
			_dayOffBusinessRuleValidation = dayOffBusinessRuleValidation;
			_currentUnitOfWork = currentUnitOfWork;
		}

		public SchedulingResultModel Execute(DateOnlyPeriod period)
		{
			var stateHolder = _schedulerStateHolder();
			var fixedStaffPeople = stateHolder.SchedulingResultState.PersonsInOrganization.FixedStaffPeople(period).ToList();
			//some hack to get rid of lazy load ex
			var uow = _currentUnitOfWork.Current();
			uow.Reassociate(fixedStaffPeople);
			stateHolder.Schedules.ForEach(range => range.Value.Reassociate(uow));
			//
			var scheduleOfSelectedPeople = stateHolder.Schedules.Where(x => fixedStaffPeople.Contains(x.Key)).ToList();
			var violatedBusinessRules = new List<BusinessRulesValidationResult>();

			var schedulePeriodNotInRange = _violatedSchedulePeriodBusinessRule.GetResult(fixedStaffPeople, period).ToList();
			var daysOffValidationResult = getDayOffBusinessRulesValidationResults(scheduleOfSelectedPeople, schedulePeriodNotInRange, period);
			violatedBusinessRules.AddRange(schedulePeriodNotInRange);
			violatedBusinessRules.AddRange(daysOffValidationResult);
			return new SchedulingResultModel
			{
				ScheduledAgentsCount = successfulScheduledAgents(scheduleOfSelectedPeople, period),
				BusinessRulesValidationResults = violatedBusinessRules
			};
		}

		private IEnumerable<BusinessRulesValidationResult> getDayOffBusinessRulesValidationResults(
			IEnumerable<KeyValuePair<IPerson, IScheduleRange>> schedules,
			List<BusinessRulesValidationResult> schedulePeriodNotInRange,
			DateOnlyPeriod periodToCheck)
		{
			var result = new List<BusinessRulesValidationResult>();
			foreach (var item in schedules)
			{
				if (isAmongInvalidScheduleRange(schedulePeriodNotInRange, item.Key)) continue;
				if (!_dayOffBusinessRuleValidation.Validate(item.Value, periodToCheck))
				{
					result.Add(new BusinessRulesValidationResult
					{
						BusinessRuleCategory = BusinessRuleCategory.DayOff,
						//should be in resource files - not now to prevent translation
						BusinessRuleCategoryText = "Days off",
						Message =
							string.Format(UserTexts.Resources.TargetDayOffNotFulfilledMessage, item.Value.CalculatedTargetScheduleDaysOff(periodToCheck)),
						Name = item.Key.Name.ToString(NameOrderOption.FirstNameLastName)
					});
				}
				else if (!isAgentFulfillingContractTime(item.Value, periodToCheck))
				{
					result.Add(new BusinessRulesValidationResult
					{
						BusinessRuleCategory = BusinessRuleCategory.DayOff,
						BusinessRuleCategoryText = "Scheduled time",
						Message =
							string.Format(UserTexts.Resources.TargetScheduleTimeNotFullfilled, DateHelper.HourMinutesString(item.Value.CalculatedTargetTimeHolder(periodToCheck).GetValueOrDefault(TimeSpan.Zero).TotalMinutes)),
						Name = item.Key.Name.ToString(NameOrderOption.FirstNameLastName)
					});
				}
				else
				{
					var agentScheduleDaysWithoutSchedule = getAgentScheduleDaysWithoutSchedule(item.Value, periodToCheck);
					if (agentScheduleDaysWithoutSchedule > 0)
					{
						result.Add(new BusinessRulesValidationResult
						{
							BusinessRuleCategory = BusinessRuleCategory.DayOff,
							BusinessRuleCategoryText = "Scheduled time",
							Message = string.Format(UserTexts.Resources.AgentHasDaysWithoutAnySchedule, agentScheduleDaysWithoutSchedule),
							Name = item.Key.Name.ToString(NameOrderOption.FirstNameLastName)
						});
					}
				}
			}
			return result;
		}

		private static bool isAmongInvalidScheduleRange(List<BusinessRulesValidationResult> schedulePeriodNotInRange, IPerson person)
		{
			return schedulePeriodNotInRange.Contains(new BusinessRulesValidationResult
			{
				BusinessRuleCategory = BusinessRuleCategory.SchedulePeriod,
				//should be in resource files - not now to prevent translation
				BusinessRuleCategoryText = "Schedule period",
				Name = person.Name.ToString(NameOrderOption.FirstNameLastName)
			});
		}

		private static int successfulScheduledAgents(IEnumerable<KeyValuePair<IPerson, IScheduleRange>> schedules, DateOnlyPeriod periodToCheck)
		{
			return schedules.Count(x => isAgentScheduled(x.Value, periodToCheck));
		}

		private static bool isAgentScheduled(IScheduleRange scheduleRange, DateOnlyPeriod periodToCheck)
		{
			return isAgentFulfillingContractTime(scheduleRange, periodToCheck) &&
				   getAgentScheduleDaysWithoutSchedule(scheduleRange, periodToCheck) == 0;
		}

		private static bool isAgentFulfillingContractTime(IScheduleRange scheduleRange, DateOnlyPeriod periodToCheck)
		{
			var targetSummary = scheduleRange.CalculatedTargetTimeSummary(periodToCheck);
			var scheduleSummary = scheduleRange.CalculatedCurrentScheduleSummary(periodToCheck);
			return targetSummary.TargetTime.HasValue &&
				   targetSummary.TargetTime - targetSummary.NegativeTargetTimeTolerance <= scheduleSummary.ContractTime &&
				   targetSummary.TargetTime + targetSummary.PositiveTargetTimeTolerance >= scheduleSummary.ContractTime;
		}

		private static int getAgentScheduleDaysWithoutSchedule(IScheduleRange scheduleRange, DateOnlyPeriod periodToCheck)
		{
			var scheduleSummary = scheduleRange.CalculatedCurrentScheduleSummary(periodToCheck);
			return scheduleSummary.DaysWithoutSchedule;
		}
	}
}