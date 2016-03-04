using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;

		public IntradayOptimizationCommandHandler(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public virtual void Execute(IntradayOptimizationCommand command)
		{
			_eventPublisher.Publish(new OptimizationWasOrdered
			{
				Period = command.Period,
				AgentIds = command.Agents.Select(x => x.Id.Value)
			});
		}
	}
}