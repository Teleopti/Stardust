using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly OptimizationResult _optimizationResult;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;
		private readonly IScheduleDictionaryPersister _persister;
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

		public IntradayOptimizationCommandHandler(IEventPublisher eventPublisher, 
																			OptimizationResult optimizationResult, 
																			IPlanningPeriodRepository planningPeriodRepository,
																			IScheduleDictionaryPersister persister,
																			Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_eventPublisher = eventPublisher;
			_optimizationResult = optimizationResult;
			_planningPeriodRepository = planningPeriodRepository;
			_persister = persister;
			_schedulerStateHolder = schedulerStateHolder;
		}

		public virtual OptimizationResultModel Execute(Guid planningPeriodId, IEnumerable<Guid> personIds)
		{
			var optimizationResult = DoOptimization(planningPeriodId, personIds);
			_persister.Persist(_schedulerStateHolder().Schedules);
			return optimizationResult;
		}

		[UnitOfWork]
		protected virtual OptimizationResultModel DoOptimization(Guid planningPeriodId, IEnumerable<Guid> personIds)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;
			
			_eventPublisher.Publish(new OptimizationWasOrdered { Period = period, AgentIds = personIds });
			return _optimizationResult.Create(period);
		}
	}
}