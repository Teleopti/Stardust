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
			Setup(period, people);
			_schedulingCommandHandler.Execute(new SchedulingCommand
			{
				Period = period,
				RunWeeklyRestSolver = false
			});
			_persister.Persist(stateHolder.Schedules);
			return CreateResult(period);
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