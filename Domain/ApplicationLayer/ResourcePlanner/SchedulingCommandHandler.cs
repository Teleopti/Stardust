using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class SchedulingCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly IGridlockManager _gridLockManager;
		private readonly CreateIslands _createIslands;
		private readonly ReduceSkillGroups _reduceSkillGroups;
		private readonly IPeopleInOrganization _peopleInOrganization;

		public SchedulingCommandHandler(IEventPublisher eventPublisher, IGridlockManager gridLockManager, CreateIslands createIslands, ReduceSkillGroups reduceSkillGroups, IPeopleInOrganization peopleInOrganization)
		{
			_eventPublisher = eventPublisher;
			_gridLockManager = gridLockManager;
			_createIslands = createIslands;
			_reduceSkillGroups = reduceSkillGroups;
			_peopleInOrganization = peopleInOrganization;
		}

		public void Execute(SchedulingCommand command)
		{
			var events = new List<IEvent>();
			var islands = CreateIslands(command.Period, command);
			var userLocks = _gridLockManager.LockInfos();
			foreach (var island in islands)
			{
				var agentsInIsland = island.AgentsInIsland();
				var agentsToSchedule = (command.AgentsToSchedule?.Where(x => agentsInIsland.Contains(x)).ToArray() ?? agentsInIsland)
					.FixedStaffPeople(command.Period);

				if (agentsToSchedule.Any())
				{
					var @event = new SchedulingWasOrdered
					{
						AgentsToSchedule = agentsToSchedule.Select(x => x.Id.Value),
						AgentsInIsland = agentsInIsland.Select(x => x.Id.Value),
						StartDate = command.Period.StartDate,
						EndDate = command.Period.EndDate,
						RunWeeklyRestSolver = command.RunWeeklyRestSolver,
						CommandId = command.CommandId,
						UserLocks = userLocks
					};
					events.Add(@event);
				}
			}

			_eventPublisher.Publish(events.ToArray());
		}

		[UnitOfWork]
		protected virtual IEnumerable<Island> CreateIslands(DateOnlyPeriod period, SchedulingCommand command)
		{
			using (CommandScope.Create(command))
			{
				return _createIslands.Create(_reduceSkillGroups, _peopleInOrganization.Agents(period), period);
			}
		}
	}
}