using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class FullScheduling
	{
		private readonly SchedulingCommandHandler _schedulingCommandHandler;
		private readonly FillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly SchedulingInformationProvider _schedulingInformationProvider;
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;
		private readonly FullSchedulingResult _fullSchedulingResult;

		public FullScheduling(SchedulingCommandHandler schedulingCommandHandler, 
			FillSchedulerStateHolder fillSchedulerStateHolder,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			SchedulingInformationProvider schedulingInformationProvider,
			ISchedulingOptionsProvider schedulingOptionsProvider, 
			FullSchedulingResult fullSchedulingResult)
		{
			_schedulingCommandHandler = schedulingCommandHandler;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_schedulerStateHolder = schedulerStateHolder;
			_schedulingInformationProvider = schedulingInformationProvider;
			_schedulingOptionsProvider = schedulingOptionsProvider;
			_fullSchedulingResult = fullSchedulingResult;
		}

		//runDayOffOptimization here for test purposes
		public FullSchedulingResultModel DoScheduling(Guid planningPeriodId, bool runDayOffOptimization = true)
		{			
			var schedulingInformation = _schedulingInformationProvider.GetInfoFromPlanningPeriod(planningPeriodId);
			var stateHolder = _schedulerStateHolder();
			var schedulingOptions = _schedulingOptionsProvider.Fetch(null);
			Setup(schedulingInformation.Period, schedulingInformation.PersonIds);
			_schedulingCommandHandler.Execute(new SchedulingCommand
			{
				Period = schedulingInformation.Period,
				RunWeeklyRestSolver = false,
				FromWeb = true,
				AgentsToSchedule = stateHolder.SchedulingResultState.LoadedAgents,
				PlanningPeriodId = planningPeriodId,
				RunDayOffOptimization = runDayOffOptimization
			});
			return _fullSchedulingResult.Create(schedulingInformation.Period, stateHolder.SchedulingResultState.LoadedAgents, schedulingInformation.PlanningGroup, schedulingOptions.UsePreferences);
			
		}

		[TestLog]
		[UnitOfWork]
		protected virtual void Setup(DateOnlyPeriod period, IEnumerable<Guid> people)
		{
			_fillSchedulerStateHolder.Fill(_schedulerStateHolder(), people, null, period);
		}
	}
}