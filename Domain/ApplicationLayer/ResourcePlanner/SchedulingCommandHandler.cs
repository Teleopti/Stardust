using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class SchedulingCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly IGridlockManager _gridLockManager;

		public SchedulingCommandHandler(IEventPublisher eventPublisher, IGridlockManager gridLockManager)
		{
			_eventPublisher = eventPublisher;
			_gridLockManager = gridLockManager;
		}

		public void Execute(SchedulingCommand schedulingCommand)
		{
			var @event = new SchedulingWasOrdered
			{
				AgentsToSchedule = schedulingCommand.AgentsToSchedule.Select(x => x.Id.Value),
				StartDate = schedulingCommand.Period.StartDate,
				EndDate = schedulingCommand.Period.EndDate,
				RunWeeklyRestSolver = schedulingCommand.RunWeeklyRestSolver,
				CommandId = schedulingCommand.CommandId,
				UserLocks = _gridLockManager.LockInfos()
			};
			_eventPublisher.Publish(@event);
		}
	}
}