using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class FullScheduling
	{
		private readonly SchedulingCommandHandler _schedulingCommandHandler;
		private readonly IFillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly FullSchedulingResult _fullSchedulingResult;

		public FullScheduling(SchedulingCommandHandler schedulingCommandHandler, IFillSchedulerStateHolder fillSchedulerStateHolder,
			Func<ISchedulerStateHolder> schedulerStateHolder, IScheduleDictionaryPersister persister, FullSchedulingResult fullSchedulingResult)
		{
			_schedulingCommandHandler = schedulingCommandHandler;
			_schedulingCommandHandler = schedulingCommandHandler;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_schedulerStateHolder = schedulerStateHolder;
			_persister = persister;
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
			_fillSchedulerStateHolder.Fill(stateHolder, people, null, period);
			
			if (stateHolder.Schedules.Any())
			{
				ExecuteScheduling(period, stateHolder);
			}
		}

		[RemoveMeWithToggle("move up this", Toggles.ResourcePlanner_SchedulingIslands_44757)]
		protected virtual void ExecuteScheduling(DateOnlyPeriod period, ISchedulerStateHolder stateHolder)
		{
			var command = new SchedulingCommand
			{
				AgentsToSchedule = stateHolder.SchedulingResultState.PersonsInOrganization,
				Period = period,
				RunWeeklyRestSolver = false
			};

			_schedulingCommandHandler.Execute(command);
		}
	}



	[RemoveMeWithToggle(Toggles.ResourcePlanner_SchedulingIslands_44757)]
	public class FullSchedulingOLD : FullScheduling
	{
		private readonly IScheduleExecutor _scheduleExecutor;
		private readonly ISchedulingProgress _schedulingProgress;
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;

		public FullSchedulingOLD(IScheduleExecutor scheduleExecutor, IFillSchedulerStateHolder fillSchedulerStateHolder, Func<ISchedulerStateHolder> schedulerStateHolder, IScheduleDictionaryPersister persister, ISchedulingProgress schedulingProgress, ISchedulingOptionsProvider schedulingOptionsProvider, FullSchedulingResult fullSchedulingResult) 
			: base(null, fillSchedulerStateHolder, schedulerStateHolder, persister, fullSchedulingResult)
		{
			_scheduleExecutor = scheduleExecutor;
			_schedulingProgress = schedulingProgress;
			_schedulingOptionsProvider = schedulingOptionsProvider;
		}

		protected override void ExecuteScheduling(DateOnlyPeriod period, ISchedulerStateHolder stateHolder)
		{
			_scheduleExecutor.Execute(new NoSchedulingCallback(), _schedulingOptionsProvider.Fetch(), _schedulingProgress,
				stateHolder.SchedulingResultState.PersonsInOrganization.FixedStaffPeople(period), period, false);
		}
	}
}