using System.Linq;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulingCommandHandler
	{
		private readonly SchedulingEventHandler _schedulingEventHandler;

		public SchedulingCommandHandler(SchedulingEventHandler schedulingEventHandler)
		{
			_schedulingEventHandler = schedulingEventHandler;
		}

		public void Execute(SchedulingCommand schedulingCommand, ISchedulingProgress backgroundWorker)
		{
			var @event = new SchedulingWasOrdered
			{
				AgentsToSchedule = schedulingCommand.AgentsToSchedule.Select(x => x.Id.Value),
				StartDate = schedulingCommand.Period.StartDate,
				EndDate = schedulingCommand.Period.EndDate,
				RunWeeklyRestSolver = schedulingCommand.RunWeeklyRestSolver,
				CommandId = schedulingCommand.CommandId
			};
			//use event publisher here (when we removed params except event here)
			_schedulingEventHandler.HandleEvent(@event, backgroundWorker);
		}
	}
}