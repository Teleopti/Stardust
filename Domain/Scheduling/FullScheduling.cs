using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class FullScheduling
	{
		private readonly WebSchedulingSetup _webSchedulingSetup;
		private readonly Func<IScheduleCommand> _scheduleCommand;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IRequiredScheduleHelper> _requiredScheduleHelper;
		private readonly Func<IGroupPagePerDateHolder> _groupPagePerDateHolder;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly ViolatedSchedulePeriodBusinessRule _violatedSchedulePeriodBusinessRule;
		private readonly DayOffBusinessRuleValidation _dayOffBusinessRuleValidation;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public FullScheduling(WebSchedulingSetup webSchedulingSetup,
			Func<IScheduleCommand> scheduleCommand, Func<ISchedulerStateHolder> schedulerStateHolder,
			Func<IRequiredScheduleHelper> requiredScheduleHelper, Func<IGroupPagePerDateHolder> groupPagePerDateHolder,
			IScheduleDictionaryPersister persister, ViolatedSchedulePeriodBusinessRule violatedSchedulePeriodBusinessRule,
			DayOffBusinessRuleValidation dayOffBusinessRuleValidation, ICurrentUnitOfWork currentUnitOfWork)
		{
			_webSchedulingSetup = webSchedulingSetup;
			_scheduleCommand = scheduleCommand;
			_schedulerStateHolder = schedulerStateHolder;
			_requiredScheduleHelper = requiredScheduleHelper;
			_groupPagePerDateHolder = groupPagePerDateHolder;
			_persister = persister;
			_violatedSchedulePeriodBusinessRule = violatedSchedulePeriodBusinessRule;
			_dayOffBusinessRuleValidation = dayOffBusinessRuleValidation;
			_currentUnitOfWork = currentUnitOfWork;
		}

		public virtual SchedulingResultModel DoScheduling(DateOnlyPeriod period)
		{
			var people = SetupAndSchedule(period);
			_persister.Persist(_schedulerStateHolder().Schedules);
			return CreateResult(period, people);
		}

		[LogTime]
		[UnitOfWork]
		protected virtual SchedulingResultModel CreateResult(DateOnlyPeriod period, PeopleSelection people)
		{
			//some hack to get rid of lazy load ex
			var uow = _currentUnitOfWork.Current();
			uow.Reassociate(people.FixedStaffPeople);
			_schedulerStateHolder().Schedules.ForEach(range => range.Value.Reassociate(uow));
			//
			var scheduleOfSelectedPeople = _schedulerStateHolder().Schedules.Where(x => people.FixedStaffPeople.Contains(x.Key)).ToList();
			var voilatedBusinessRules = new List<BusinessRulesValidationResult>();

			var schedulePeriodNotInRange = _violatedSchedulePeriodBusinessRule.GetResult(people.FixedStaffPeople, period).ToList();
			var daysOffValidationResult = getDayOffBusinessRulesValidationResults(scheduleOfSelectedPeople, schedulePeriodNotInRange, period);
			voilatedBusinessRules.AddRange(schedulePeriodNotInRange);
			voilatedBusinessRules.AddRange(daysOffValidationResult);
			return new SchedulingResultModel
			{
				ScheduledAgentsCount = successfulScheduledAgents(scheduleOfSelectedPeople, period),
				BusinessRulesValidationResults = voilatedBusinessRules
			};
		}

		[LogTime]
		[UnitOfWork]
		protected virtual PeopleSelection SetupAndSchedule(DateOnlyPeriod period)
		{
			var webScheduleState = _webSchedulingSetup.Setup(period);

			if (webScheduleState.AllSchedules.Any())
			{
				_scheduleCommand().Execute(new OptimizerOriginalPreferences(new SchedulingOptions
				{
					UseAvailability = true,
					UsePreferences = true,
					UseRotations = true,
					UseStudentAvailability = false,
					DayOffTemplate = _schedulerStateHolder().CommonStateHolder.DefaultDayOffTemplate,
					ScheduleEmploymentType = ScheduleEmploymentType.FixedStaff,
					GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy),
					TagToUseOnScheduling = NullScheduleTag.Instance
				}), new NoBackgroundWorker(), _schedulerStateHolder(), webScheduleState.AllSchedules, _groupPagePerDateHolder(),
					_requiredScheduleHelper(),
					new OptimizationPreferences(), false, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));
			}

			return webScheduleState.PeopleSelection;
		}

		private static int successfulScheduledAgents(IEnumerable<KeyValuePair<IPerson, IScheduleRange>> schedules, DateOnlyPeriod periodToCheck)
		{
			return
				schedules.Count(
					x =>
					{
						var calculatedTargetTime = x.Value.CalculatedTargetTimeHolder(periodToCheck);
						return calculatedTargetTime.HasValue && x.Value.CalculatedContractTimeHolderOnPeriod(periodToCheck) == calculatedTargetTime;
					});
		}

		private static bool isAgentScheduled(IScheduleRange scheduleRange, DateOnlyPeriod periodToCheck)
		{
			var targetTime = scheduleRange.CalculatedTargetTimeHolder(periodToCheck);
			return targetTime.HasValue &&
				(targetTime.Value == scheduleRange.CalculatedContractTimeHolderOnPeriod(periodToCheck));
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
							string.Format("Target of {0} scheduled time is not fulfilled",
								DateHelper.HourMinutesString(
									item.Value.CalculatedTargetTimeHolder(periodTocheck).GetValueOrDefault(TimeSpan.Zero).TotalMinutes)),
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