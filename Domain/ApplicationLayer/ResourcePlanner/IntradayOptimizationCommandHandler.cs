using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;

		public IntradayOptimizationCommandHandler(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public virtual void Execute(DateOnlyPeriod period, IEnumerable<IPerson> agents)
		{
			_eventPublisher.Publish(new OptimizationWasOrdered { Period = period, AgentIds = agents.Select(x => x.Id.Value) });
		}
	}
}