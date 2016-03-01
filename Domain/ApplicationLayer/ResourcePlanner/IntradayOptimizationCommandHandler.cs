using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;

		public IntradayOptimizationCommandHandler(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Execute(Guid planningPeriodId)
		{
			_eventPublisher.Publish(new OptimizationWasOrdered {PlanningPeriodId = planningPeriodId});
		}
	}
}