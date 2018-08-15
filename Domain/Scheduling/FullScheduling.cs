using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		public FullScheduling(SchedulingCommandHandler schedulingCommandHandler, 
			FillSchedulerStateHolder fillSchedulerStateHolder,
			Func<ISchedulerStateHolder> schedulerStateHolder,
			SchedulingInformationProvider schedulingInformationProvider)
		{
			_schedulingCommandHandler = schedulingCommandHandler;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_schedulerStateHolder = schedulerStateHolder;
			_schedulingInformationProvider = schedulingInformationProvider;
		}

		public virtual void DoScheduling(Guid planningPeriodId)
		{
			var schedulingInformation = _schedulingInformationProvider.GetInfoFromPlanningPeriod(planningPeriodId);
			var stateHolder = _schedulerStateHolder();
			Setup(schedulingInformation.Period, schedulingInformation.PersonIds);
			_schedulingCommandHandler.Execute(new SchedulingCommand
			{
				Period = schedulingInformation.Period,
				RunWeeklyRestSolver = false,
				FromWeb = true,
				AgentsToSchedule = stateHolder.SchedulingResultState.LoadedAgents,
				PlanningPeriodId = planningPeriodId
			});
		}

		[TestLog]
		[UnitOfWork]
		protected virtual void Setup(DateOnlyPeriod period, IEnumerable<Guid> people)
		{
			_fillSchedulerStateHolder.Fill(_schedulerStateHolder(), people, null, period);
		}
	}
}