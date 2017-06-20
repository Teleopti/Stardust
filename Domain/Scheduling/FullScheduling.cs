using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class FullScheduling
	{
		private readonly IFillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly IScheduleExecutor _scheduleExecutor;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly ISchedulingProgress _schedulingProgress;
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;
		private readonly FullSchedulingResult _fullSchedulingResult;

		public FullScheduling(IFillSchedulerStateHolder fillSchedulerStateHolder,
			IScheduleExecutor scheduleExecutor, Func<ISchedulerStateHolder> schedulerStateHolder,
			IScheduleDictionaryPersister persister, ISchedulingProgress schedulingProgress, 
			ISchedulingOptionsProvider schedulingOptionsProvider, FullSchedulingResult fullSchedulingResult)
		{
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_scheduleExecutor = scheduleExecutor;
			_schedulerStateHolder = schedulerStateHolder;
			_persister = persister;
			_schedulingProgress = schedulingProgress;
			_schedulingOptionsProvider = schedulingOptionsProvider;
			_fullSchedulingResult = fullSchedulingResult;
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
			return _fullSchedulingResult.Execute(period);
		}

		[TestLog]
		[UnitOfWork]
		protected virtual void SetupAndSchedule(DateOnlyPeriod period, IEnumerable<Guid> people)
		{
			var stateHolder = _schedulerStateHolder();
			_fillSchedulerStateHolder.Fill(stateHolder, people, null, null, period);
			
			if (stateHolder.Schedules.Any())
			{
				_scheduleExecutor.Execute(new NoSchedulingCallback(), _schedulingOptionsProvider.Fetch(), _schedulingProgress,
					stateHolder.SchedulingResultState.PersonsInOrganization.FixedStaffPeople(period), period,
					new OptimizationPreferences(), false, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));
			}
		}
	}
}