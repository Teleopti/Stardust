using System.Linq;
using Teleopti.Ccc.Domain.DayOffPlanning;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationCommandHandler : IIntradayOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly CreateIslands _createIslands;

		public IntradayOptimizationCommandHandler(IEventPublisher eventPublisher, CreateIslands createIslands)
		{
			_eventPublisher = eventPublisher;
			_createIslands = createIslands;
		}

		public void Execute(IntradayOptimizationCommand command)
		{
			foreach (var island in _createIslands.Create(command.Period, command.Agents))
			{
				_eventPublisher.Publish(new OptimizationWasOrdered
				{
					Period = command.Period,
					AgentIds = island.PersonsInIsland().Select(x => x.Id.Value)
				});
			}
		}
	}

	//remove below when toggle 36939 is removed
	public interface IIntradayOptimizationCommandHandler
	{
		void Execute(IntradayOptimizationCommand command);
	}
	public class IntradayOptimizationOneThreadCommandHandler : IIntradayOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;

		public IntradayOptimizationOneThreadCommandHandler(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Execute(IntradayOptimizationCommand command)
		{
			_eventPublisher.Publish(new OptimizationWasOrdered
			{
				Period = command.Period,
				AgentIds = command.Agents.Select(x => x.Id.Value),
				RunResolveWeeklyRestRule = command.RunResolveWeeklyRestRule
	});
		}
	}
}