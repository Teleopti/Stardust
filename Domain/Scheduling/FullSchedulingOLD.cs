using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{ 
	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
	public interface IFullScheduling
	{
		SchedulingResultModel DoScheduling(DateOnlyPeriod period);
		SchedulingResultModel DoScheduling(DateOnlyPeriod period, IEnumerable<Guid> people);
		SchedulingResultModel DoScheduling(DateOnlyPeriod period, IEnumerable<Guid> people, Guid planningPeriodId);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
	public class FullSchedulingOLD : IFullScheduling
	{
		private readonly IScheduleExecutor _scheduleExecutor;
		private readonly IFillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly ISchedulingProgress _schedulingProgress;
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;
		private readonly FullSchedulingResult _fullSchedulingResult;
		private readonly ISchedulingSourceScope _schedulingSourceScope;

		public FullSchedulingOLD(IScheduleExecutor scheduleExecutor, IFillSchedulerStateHolder fillSchedulerStateHolder, Func<ISchedulerStateHolder> schedulerStateHolder, IScheduleDictionaryPersister persister, ISchedulingProgress schedulingProgress, ISchedulingOptionsProvider schedulingOptionsProvider, FullSchedulingResult fullSchedulingResult, ISchedulingSourceScope schedulingSourceScope) 
		{
			_scheduleExecutor = scheduleExecutor;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_schedulerStateHolder = schedulerStateHolder;
			_persister = persister;
			_schedulingProgress = schedulingProgress;
			_schedulingOptionsProvider = schedulingOptionsProvider;
			_fullSchedulingResult = fullSchedulingResult;
			_schedulingSourceScope = schedulingSourceScope;
		}

		public virtual SchedulingResultModel DoScheduling(DateOnlyPeriod period)
		{
			return DoScheduling(period, null);
		}
		public virtual SchedulingResultModel DoScheduling(DateOnlyPeriod period, IEnumerable<Guid> people)
		{
			using (_schedulingSourceScope.OnThisThreadUse(ScheduleSource.WebScheduling))
			{
				var stateHolder = _schedulerStateHolder();
				SetupAndSchedule(period, people);
				_persister.Persist(stateHolder.Schedules);
				return CreateResult(period);
			}
		}

		public virtual SchedulingResultModel DoScheduling(DateOnlyPeriod period, IEnumerable<Guid> people, Guid planningPeriodId)
		{
			return DoScheduling(period, people);
		}

		[TestLog]
		[UnitOfWork]
		protected virtual SchedulingResultModel CreateResult(DateOnlyPeriod period)
		{
			return _fullSchedulingResult.Execute(period, _schedulerStateHolder().SchedulingResultState.PersonsInOrganization.FixedStaffPeople(period).ToList());
		}

		[TestLog]
		[UnitOfWork]
		protected virtual void SetupAndSchedule(DateOnlyPeriod period, IEnumerable<Guid> people)
		{
			var stateHolder = _schedulerStateHolder();
			_fillSchedulerStateHolder.Fill(stateHolder, people, null, period);

			if (period.StartDate.Day == 1 && period.EndDate.AddDays(1).Day == 1 && stateHolder.SchedulingResultState.PersonsInOrganization.FixedStaffPeople(period).Any())
			{
				var firstDaysOfWeek = new List<DayOfWeek>();
				foreach (var person in stateHolder.SchedulingResultState.PersonsInOrganization.FixedStaffPeople(period))
				{
					if (!firstDaysOfWeek.Contains(person.FirstDayOfWeek))
					{
						firstDaysOfWeek.Add(person.FirstDayOfWeek);
					}
				}

				var firstDateInPeriodLocal = DateHelper.GetFirstDateInWeek(period.StartDate, firstDaysOfWeek[0]);
				var lastDateInPeriodLocal = DateHelper.GetLastDateInWeek(period.EndDate, firstDaysOfWeek[0]);
				foreach (var firstDayOfWeek in firstDaysOfWeek)
				{
					if (DateHelper.GetFirstDateInWeek(period.StartDate, firstDayOfWeek).CompareTo(firstDateInPeriodLocal) != 1)
					{
						firstDateInPeriodLocal = DateHelper.GetFirstDateInWeek(period.StartDate, firstDayOfWeek);
					}
					if (DateHelper.GetLastDateInWeek(period.EndDate, firstDayOfWeek).CompareTo(lastDateInPeriodLocal) == 1)
					{
						lastDateInPeriodLocal = DateHelper.GetLastDateInWeek(period.EndDate, firstDayOfWeek);
					}
				}
				period = new DateOnlyPeriod(firstDateInPeriodLocal, lastDateInPeriodLocal);
			}

			var schedulinOptions = _schedulingOptionsProvider.Fetch(stateHolder.CommonStateHolder.DefaultDayOffTemplate);
			_scheduleExecutor.Execute(new NoSchedulingCallback(), schedulinOptions, _schedulingProgress,
				stateHolder.SchedulingResultState.PersonsInOrganization.FixedStaffPeople(period), period, false,new FixedBlockPreferenceProvider(new ExtraPreferences()
				{
					UseBlockSameStartTime = schedulinOptions.BlockSameStartTime,
					UseBlockSameShift = schedulinOptions.BlockSameShift,
					UseBlockSameShiftCategory = schedulinOptions.BlockSameShiftCategory,
					BlockTypeValue = schedulinOptions.BlockFinderTypeForAdvanceScheduling
				}));
		}
	}
}