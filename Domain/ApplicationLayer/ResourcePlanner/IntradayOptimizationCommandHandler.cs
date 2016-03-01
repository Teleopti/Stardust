using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly OptimizationResult _optimizationResult;
		private readonly IPlanningPeriodRepository _planningPeriodRepository;

		public IntradayOptimizationCommandHandler(IEventPublisher eventPublisher, OptimizationResult optimizationResult, IPlanningPeriodRepository planningPeriodRepository)
		{
			_eventPublisher = eventPublisher;
			_optimizationResult = optimizationResult;
			_planningPeriodRepository = planningPeriodRepository;
		}

		public OptimizationResultModel Execute(Guid planningPeriodId)
		{
			var planningPeriod = _planningPeriodRepository.Load(planningPeriodId);
			var period = planningPeriod.Range;
			_eventPublisher.Publish(new OptimizationWasOrdered {PlanningPeriodId = planningPeriodId});
			return _optimizationResult.Create(period);
		}
	}
}