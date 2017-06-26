using System.Linq;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class SchedulingCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;

		public SchedulingCommandHandler(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Execute(SchedulingCommand schedulingCommand)
		{
			var @event = new SchedulingWasOrdered
			{
				AgentsToSchedule = schedulingCommand.AgentsToSchedule.Select(x => x.Id.Value),
				StartDate = schedulingCommand.Period.StartDate,
				EndDate = schedulingCommand.Period.EndDate,
				RunWeeklyRestSolver = schedulingCommand.RunWeeklyRestSolver,
				CommandId = schedulingCommand.CommandId
			};
			_eventPublisher.Publish(@event);
		}
	}
}