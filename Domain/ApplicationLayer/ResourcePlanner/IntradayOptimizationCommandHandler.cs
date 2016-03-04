using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly OptimizationResult _optimizationResult;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;

		public IntradayOptimizationCommandHandler(IEventPublisher eventPublisher, 
																			OptimizationResult optimizationResult, 
																			IPlanningPeriodRepository planningPeriodRepository)
		{
			_eventPublisher = eventPublisher;
			_optimizationResult = optimizationResult;
			_planningPeriodRepository = planningPeriodRepository;
		}

		public virtual OptimizationResultModel Execute(Guid planningPeriodId, IEnumerable<Guid> personIds)
		{
			return DoOptimization(planningPeriodId, personIds);
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