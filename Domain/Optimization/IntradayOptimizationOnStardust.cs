using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class IntradayOptimizationOnStardust
	{
		private readonly IEventPublisher _eventPublisher;

		public IntradayOptimizationOnStardust(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}
	
		public void Execute(Guid planningPeriodId)
		{
			_eventPublisher.Publish(new WebIntradayOptimizationStardustEvent
			{
				PlanningPeriodId = planningPeriodId
			});
		}
	}
}