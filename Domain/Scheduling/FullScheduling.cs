using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
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
		private readonly FullSchedulingResult _fullSchedulingResult;
		private readonly SchedulingInformationProvider _schedulingInformationProvider;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public FullScheduling(SchedulingCommandHandler schedulingCommandHandler, 
			IFillSchedulerStateHolder fillSchedulerStateHolder,
			Func<ISchedulerStateHolder> schedulerStateHolder, 
			FullSchedulingResult fullSchedulingResult,
			SchedulingInformationProvider schedulingInformationProvider,
			ICurrentUnitOfWork currentUnitOfWork)
		{
			_schedulingCommandHandler = schedulingCommandHandler;
			_fillSchedulerStateHolder = fillSchedulerStateHolder;
			_schedulerStateHolder = schedulerStateHolder;
			_fullSchedulingResult = fullSchedulingResult;
			_schedulingInformationProvider = schedulingInformationProvider;
			_currentUnitOfWork = currentUnitOfWork;
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
				AgentsToSchedule = stateHolder.AllPermittedPersons,
				PlanningPeriodId = planningPeriodId
			});
			return CreateResult(stateHolder.AllPermittedPersons, schedulingInformation.Period);
		}

		[TestLog]
		[UnitOfWork]
		protected virtual SchedulingResultModel CreateResult(IEnumerable<IPerson> fixedStaffPeople, DateOnlyPeriod period)
		{
			_currentUnitOfWork.Current().Reassociate(fixedStaffPeople);
			return _fullSchedulingResult.Execute(period, fixedStaffPeople);
		}

		[TestLog]
		[UnitOfWork]
		protected virtual void Setup(DateOnlyPeriod period, IEnumerable<Guid> people)
		{
			_fillSchedulerStateHolder.Fill(_schedulerStateHolder(), people, null, period);
		}
	}
}