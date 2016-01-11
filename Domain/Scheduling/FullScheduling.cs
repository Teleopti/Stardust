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
	//This class is just a mess. Copied from ScheduleController. Needs a lot of refactoring...
	public class FullScheduling
	{
		private readonly SetupStateHolderForWebScheduling _setupStateHolderForWebScheduling;
		private readonly IFixedStaffLoader _fixedStaffLoader;
		private readonly IScheduleControllerPrerequisites _prerequisites;
		private readonly Func<IFixedStaffSchedulingService> _fixedStaffSchedulingService;
		private readonly Func<IScheduleCommand> _scheduleCommand;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly Func<IRequiredScheduleHelper> _requiredScheduleHelper;
		private readonly Func<IGroupPagePerDateHolder> _groupPagePerDateHolder;
		private readonly Func<IScheduleTagSetter> _scheduleTagSetter;
		private readonly Func<IPersonSkillProvider> _personSkillProvider;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly ViolatedSchedulePeriodBusinessRule _violatedSchedulePeriodBusinessRule;
		private readonly DayOffBusinessRuleValidation _dayOffBusinessRuleValidation;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public FullScheduling(SetupStateHolderForWebScheduling setupStateHolderForWebScheduling,
			IFixedStaffLoader fixedStaffLoader, IScheduleControllerPrerequisites prerequisites, Func<IFixedStaffSchedulingService> fixedStaffSchedulingService,
			Func<IScheduleCommand> scheduleCommand, Func<ISchedulerStateHolder> schedulerStateHolder,
			Func<IRequiredScheduleHelper> requiredScheduleHelper, Func<IGroupPagePerDateHolder> groupPagePerDateHolder,
			Func<IScheduleTagSetter> scheduleTagSetter,
			Func<IPersonSkillProvider> personSkillProvider, IScheduleDictionaryPersister persister,
			ViolatedSchedulePeriodBusinessRule violatedSchedulePeriodBusinessRule,
			DayOffBusinessRuleValidation dayOffBusinessRuleValidation, ICurrentUnitOfWork currentUnitOfWork)
		{
			_setupStateHolderForWebScheduling = setupStateHolderForWebScheduling;
			_fixedStaffLoader = fixedStaffLoader;
			_prerequisites = prerequisites;
			_fixedStaffSchedulingService = fixedStaffSchedulingService;
			_scheduleCommand = scheduleCommand;
			_schedulerStateHolder = schedulerStateHolder;
			_requiredScheduleHelper = requiredScheduleHelper;
			_groupPagePerDateHolder = groupPagePerDateHolder;
			_scheduleTagSetter = scheduleTagSetter;
			_personSkillProvider = personSkillProvider;
			_persister = persister;
			_violatedSchedulePeriodBusinessRule = violatedSchedulePeriodBusinessRule;
			_dayOffBusinessRuleValidation = dayOffBusinessRuleValidation;
			_currentUnitOfWork = currentUnitOfWork;
		}

		[LogTime]
		public virtual SchedulingResultModel DoScheduling(DateOnlyPeriod period)
		{
			int daysScheduled;
			var people = SetupAndSchedule(period, out daysScheduled);

			var conflicts = Persist();

			return CreateResult(period, people, daysScheduled, conflicts);
		}

		[LogTime]
		[UnitOfWork]
		protected virtual SchedulingResultModel CreateResult(DateOnlyPeriod period, PeopleSelection people, int daysScheduled, IEnumerable<PersistConflict> conflicts)
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
				DaysScheduled = daysScheduled,
				ConflictCount = conflicts.Count(),
				ScheduledAgentsCount = successfulScheduledAgents(scheduleOfSelectedPeople, period),
				BusinessRulesValidationResults = voilatedBusinessRules
			};
		}

		[LogTime]
		protected virtual IEnumerable<PersistConflict> Persist()
		{
			return _persister.Persist(_schedulerStateHolder().Schedules);
		}

		[LogTime]
		[UnitOfWork]
		protected virtual PeopleSelection SetupAndSchedule(DateOnlyPeriod period, out int daysScheduled)
		{
			_prerequisites.MakeSureLoaded();

			var people = _fixedStaffLoader.Load(period);

			_setupStateHolderForWebScheduling.Setup(period, people);

			var allSchedules = extractAllSchedules(_schedulerStateHolder().SchedulingResultState, people, period);

			initializePersonSkillProviderBeforeAccessingItFromOtherThreads(period, people.AllPeople);
			_scheduleTagSetter().ChangeTagToSet(NullScheduleTag.Instance);

			var daysScheduledInternal = 0;
			if (allSchedules.Any())
			{
				EventHandler<SchedulingServiceBaseEventArgs> schedulingServiceOnDayScheduled = (sender, args) => daysScheduledInternal++;
				var fixedStaffSchedulingService = _fixedStaffSchedulingService();
				fixedStaffSchedulingService.DayScheduled += schedulingServiceOnDayScheduled;

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
				}), new NoBackgroundWorker(), _schedulerStateHolder(), allSchedules, _groupPagePerDateHolder(),
					_requiredScheduleHelper(),
					new OptimizationPreferences(), false, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));
				fixedStaffSchedulingService.DayScheduled -= schedulingServiceOnDayScheduled;
			}
			daysScheduled = daysScheduledInternal;
			return people;
		}

		private static int successfulScheduledAgents(IEnumerable<KeyValuePair<IPerson, IScheduleRange>> schedules, DateOnlyPeriod periodToCheck)
		{
			return
				schedules.Count(
					x =>
					{
						var calculatedTargetTime = x.Value.CalculatedTargetTimeHolder(periodToCheck);
						return calculatedTargetTime.HasValue && x.Value.CalculatedContractTimeHolder == calculatedTargetTime;
					});
		}

		private static bool isAgentScheduled(IScheduleRange scheduleRange, DateOnlyPeriod periodToCheck)
		{
			var targetTime = scheduleRange.CalculatedTargetTimeHolder(periodToCheck);
			return targetTime.HasValue &&
					 (targetTime.Value == scheduleRange.CalculatedContractTimeHolder);
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

		private static IList<IScheduleDay> extractAllSchedules(ISchedulingResultStateHolder stateHolder,
			PeopleSelection people,
			DateOnlyPeriod period)
		{
			var allSchedules = new List<IScheduleDay>();
			foreach (var schedule in stateHolder.Schedules)
			{
				if (people.FixedStaffPeople.Contains(schedule.Key))
				{
					allSchedules.AddRange(schedule.Value.ScheduledDayCollection(period));
				}
			}
			return allSchedules;
		}

		private void initializePersonSkillProviderBeforeAccessingItFromOtherThreads(DateOnlyPeriod period, IEnumerable<IPerson> allPeople)
		{
			var provider = _personSkillProvider();
			var dayCollection = period.DayCollection();
			allPeople.ForEach(p => dayCollection.ForEach(d => provider.SkillsOnPersonDate(p, d)));
		}
	}
}