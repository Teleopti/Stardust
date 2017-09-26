using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class FullScheduling : IFullScheduling
	{
		private readonly SchedulingCommandHandler _schedulingCommandHandler;
		private readonly IFillSchedulerStateHolder _fillSchedulerStateHolder;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly FullSchedulingResult _fullSchedulingResult;
		private readonly SchedulingInformationProvider _schedulingInformationProvider;

		public FullScheduling(SchedulingCommandHandler schedulingCommandHandler, 
			IFillSchedulerStateHolder fillSchedulerStateHolder,
			Func<ISchedulerStateHolder> schedulerStateHolder, 
			IScheduleDictionaryPersister persister,
			FullSchedulingResult fullSchedulingResult,
			SchedulingInformationProvider schedulingInformationProvider)
		{
			_schedulingCommandHandler = schedulingCommandHandler;
			_schedulingCommandHandler = schedulingCommandHandler;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_schedulerStateHolder = schedulerStateHolder;
			_persister = persister;
			_fullSchedulingResult = fullSchedulingResult;
			_schedulingInformationProvider = schedulingInformationProvider;
		}

		public virtual SchedulingResultModel DoScheduling(Guid planningPeriodId)
		{
			var schedulingInformation = _schedulingInformationProvider.GetInfoFromPlanningPeriod(planningPeriodId);
			var stateHolder = _schedulerStateHolder();
			Setup(schedulingInformation.Period, schedulingInformation.PersonIds);
			_schedulingCommandHandler.Execute(new SchedulingCommand
			{
				Period = schedulingInformation.Period,
				RunWeeklyRestSolver = false,
				FromWeb = true,
				AgentsToSchedule = schedulingInformation.PersonIds,
				PlanningPeriodId = planningPeriodId
			});
			_persister.Persist(stateHolder.Schedules);
			return CreateResult(schedulingInformation.Period);
		}

		[TestLog]
		[UnitOfWork]
		protected virtual SchedulingResultModel CreateResult(DateOnlyPeriod period)
		{
			return _fullSchedulingResult.Execute(period, _schedulerStateHolder().SchedulingResultState.PersonsInOrganization.FixedStaffPeople(period).ToList());
		}

		[TestLog]
		[UnitOfWork]
		protected virtual void Setup(DateOnlyPeriod period, IEnumerable<Guid> people)
		{
			var stateHolder = _schedulerStateHolder();
			_fillSchedulerStateHolder.Fill(stateHolder, people, null, period);
		}
	}
}