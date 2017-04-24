using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class FullScheduling
	{
		private readonly IFillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly ScheduleCommand _scheduleCommand;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly ViolatedSchedulePeriodBusinessRule _violatedSchedulePeriodBusinessRule;
		private readonly DayOffBusinessRuleValidation _dayOffBusinessRuleValidation;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly ISchedulingProgress _schedulingProgress;
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;

		public FullScheduling(IFillSchedulerStateHolder fillSchedulerStateHolder,
			ScheduleCommand scheduleCommand, Func<ISchedulerStateHolder> schedulerStateHolder,
			IScheduleDictionaryPersister persister, ViolatedSchedulePeriodBusinessRule violatedSchedulePeriodBusinessRule,
			DayOffBusinessRuleValidation dayOffBusinessRuleValidation, ICurrentUnitOfWork currentUnitOfWork, 
			ISchedulingProgress schedulingProgress, ISchedulingOptionsProvider schedulingOptionsProvider)
		{
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_scheduleCommand = scheduleCommand;
			_schedulerStateHolder = schedulerStateHolder;
			_persister = persister;
			_violatedSchedulePeriodBusinessRule = violatedSchedulePeriodBusinessRule;
			_dayOffBusinessRuleValidation = dayOffBusinessRuleValidation;
			_currentUnitOfWork = currentUnitOfWork;
			_schedulingProgress = schedulingProgress;
			_schedulingOptionsProvider = schedulingOptionsProvider;
		}

		public virtual SchedulingResultModel DoScheduling(DateOnlyPeriod period)
		{
			return DoScheduling(period, null);
		}

		public virtual SchedulingResultModel DoScheduling(DateOnlyPeriod period, IEnumerable<Guid> people)
		{
			var stateHolder = _schedulerStateHolder();
			SetupAndSchedule(period, people);
			_persister.Persist(stateHolder.Schedules);
			return CreateResult(period);
		}

		[TestLog]
		[UnitOfWork]
		protected virtual SchedulingResultModel CreateResult(DateOnlyPeriod period)
		{
			var stateHolder = _schedulerStateHolder();
			var fixedStaffPeople = stateHolder.SchedulingResultState.PersonsInOrganization.FixedStaffPeople(period).ToList();
			//some hack to get rid of lazy load ex
			var uow = _currentUnitOfWork.Current();
			uow.Reassociate(fixedStaffPeople);
			stateHolder.Schedules.ForEach(range => range.Value.Reassociate(uow));
			//
			var scheduleOfSelectedPeople = stateHolder.Schedules.Where(x => fixedStaffPeople.Contains(x.Key)).ToList();
			var voilatedBusinessRules = new List<BusinessRulesValidationResult>();

			var schedulePeriodNotInRange = _violatedSchedulePeriodBusinessRule.GetResult(fixedStaffPeople, period).ToList();
			var daysOffValidationResult = getDayOffBusinessRulesValidationResults(scheduleOfSelectedPeople, schedulePeriodNotInRange, period);
			voilatedBusinessRules.AddRange(schedulePeriodNotInRange);
			voilatedBusinessRules.AddRange(daysOffValidationResult);
			return new SchedulingResultModel
			{
				ScheduledAgentsCount = successfulScheduledAgents(scheduleOfSelectedPeople, period),
				BusinessRulesValidationResults = voilatedBusinessRules
			};
		}

		[TestLog]
		[UnitOfWork]
		protected virtual void SetupAndSchedule(DateOnlyPeriod period, IEnumerable<Guid> people)
		{
			var stateHolder = _schedulerStateHolder();
			_fillSchedulerStateHolder.Fill(stateHolder, people, null, null, period);

			if (stateHolder.Schedules.Any())
			{
				_scheduleCommand.Execute(new OptimizerOriginalPreferences(_schedulingOptionsProvider.Fetch()), _schedulingProgress,
					stateHolder.Schedules.SchedulesForPeriod(period, stateHolder.SchedulingResultState.PersonsInOrganization.FixedStaffPeople(period)).ToArray(), 
					new OptimizationPreferences(), false, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));
			}
		}

		private static int successfulScheduledAgents(IEnumerable<KeyValuePair<IPerson, IScheduleRange>> schedules, DateOnlyPeriod periodToCheck)
		{
			return schedules.Count(x => isAgentScheduled(x.Value, periodToCheck));
		}

		private static bool isAgentScheduled(IScheduleRange scheduleRange, DateOnlyPeriod periodToCheck)
		{
			var summary = scheduleRange.CalculatedTargetTimeSummary(periodToCheck);
			var scheduledTime = scheduleRange.CalculatedContractTimeHolderOnPeriod(periodToCheck);
			return summary.TargetTime.HasValue && 
				summary.TargetTime.Value - summary.NegativeTargetTimeTolerance <= scheduledTime &&
				summary.TargetTime.Value + summary.PositiveTargetTimeTolerance >= scheduledTime;
		}

		private IEnumerable<BusinessRulesValidationResult> getDayOffBusinessRulesValidationResults(
			IEnumerable<KeyValuePair<IPerson, IScheduleRange>> schedules,
			List<BusinessRulesValidationResult> schedulePeriodNotInRange,
			DateOnlyPeriod periodTocheck)
		{
			var result = new List<BusinessRulesValidationResult>();
			foreach (var item in schedules)
			{
				if (isAmongInvalidScheduleRange(schedulePeriodNotInRange, item.Key)) continue;
				if (!_dayOffBusinessRuleValidation.Validate(item.Value, periodTocheck))
				{
					result.Add(new BusinessRulesValidationResult
					{
						BusinessRuleCategory = BusinessRuleCategory.DayOff,
						//should be in resource files - not now to prevent translation
						BusinessRuleCategoryText = "Days off",
						Message =
							string.Format(UserTexts.Resources.TargetDayOffNotFulfilledMessage, item.Value.CalculatedTargetScheduleDaysOff(periodTocheck)),
						Name = item.Key.Name.ToString(NameOrderOption.FirstNameLastName)
					});
				}
				else if (!isAgentScheduled(item.Value, periodTocheck))
				{
					result.Add(new BusinessRulesValidationResult
					{
						BusinessRuleCategory = BusinessRuleCategory.DayOff,
						BusinessRuleCategoryText = "Scheduled time",
						Message =
							$"Target of {DateHelper.HourMinutesString(item.Value.CalculatedTargetTimeHolder(periodTocheck).GetValueOrDefault(TimeSpan.Zero).TotalMinutes)} scheduled time is not fulfilled",
						Name = item.Key.Name.ToString(NameOrderOption.FirstNameLastName)
					});
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

	}
}