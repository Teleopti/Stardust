using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class OptimizationResult
	{
		private readonly ViolatedSchedulePeriodBusinessRule _violatedSchedulePeriodBusinessRule;
		private readonly DayOffBusinessRuleValidation _dayOffBusinessRuleValidation;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IFindSchedulesForPersons _findSchedulesForPersons;
		private readonly IUserTimeZone _userTimeZone;
		private readonly ICurrentScenario _currentScenario;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public OptimizationResult(Func<ISchedulerStateHolder> schedulerStateHolder, IFindSchedulesForPersons findSchedulesForPersons, 
			IUserTimeZone userTimeZone, ICurrentScenario currentScenario, ICurrentUnitOfWork currentUnitOfWork, 
			ViolatedSchedulePeriodBusinessRule violatedSchedulePeriodBusinessRule, DayOffBusinessRuleValidation dayOffBusinessRuleValidation)
		{
			_violatedSchedulePeriodBusinessRule = violatedSchedulePeriodBusinessRule;
			_dayOffBusinessRuleValidation = dayOffBusinessRuleValidation;
			_schedulerStateHolder = schedulerStateHolder;
			_findSchedulesForPersons = findSchedulesForPersons;
			_userTimeZone = userTimeZone;
			_currentScenario = currentScenario;
			_currentUnitOfWork = currentUnitOfWork;
		}

		[TestLog]
		[UnitOfWork]
		public virtual OptimizationResultModel Create(DateOnlyPeriod period, IEnumerable<IPerson> fixedStaffPeople)
		{
			//some hack to get rid of lazy load ex
			var uow = _currentUnitOfWork.Current();
			uow.Reassociate(fixedStaffPeople);
			//

			var resultStateHolder = _schedulerStateHolder().SchedulingResultState;
			var personsProvider = new PersonsInOrganizationProvider(fixedStaffPeople) { DoLoadByPerson = true };
			var scheduleOfSelectedPeople = _findSchedulesForPersons.FindSchedulesForPersons(
				new ScheduleDateTimePeriod(period.ToDateTimePeriod(_userTimeZone.TimeZone()), fixedStaffPeople), _currentScenario.Current(), personsProvider, 
				new ScheduleDictionaryLoadOptions(false, false, false), fixedStaffPeople);

			var violatedBusinessRules = new List<BusinessRulesValidationResult>();

			var schedulePeriodNotInRange = _violatedSchedulePeriodBusinessRule.GetResult(fixedStaffPeople, period).ToList();
			var daysOffValidationResult = getDayOffBusinessRulesValidationResults(scheduleOfSelectedPeople, schedulePeriodNotInRange, period);
			violatedBusinessRules.AddRange(schedulePeriodNotInRange);
			violatedBusinessRules.AddRange(daysOffValidationResult);

			var result = new OptimizationResultModel()
			{
				ScheduledAgentsCount = successfulScheduledAgents(scheduleOfSelectedPeople, period),
				BusinessRulesValidationResults = violatedBusinessRules
			};
			
			if (resultStateHolder.SkillDays != null)
			{
				result.Map(resultStateHolder.SkillDays, period);
			}

			return result;
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