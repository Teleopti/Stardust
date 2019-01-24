using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class SchedulingCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly IGridlockManager _gridLockManager;
		private readonly CrossAgentsAndSkills _crossAgentsAndSkills;
		private readonly CreateIslands _createIslands;
		
		private readonly ISchedulingUseTeamProvider _schedulingUseTeamProvider;

		public SchedulingCommandHandler(IEventPublisher eventPublisher, 
				IGridlockManager gridLockManager,
				CrossAgentsAndSkills crossAgentsAndSkills,
				CreateIslands createIslands, ISchedulingUseTeamProvider schedulingUseTeamProvider)
		{
			_eventPublisher = eventPublisher;
			_gridLockManager = gridLockManager;
			_crossAgentsAndSkills = crossAgentsAndSkills;
			_createIslands = createIslands;
			_schedulingUseTeamProvider = schedulingUseTeamProvider;
		}

		[TestLog]
		public virtual void Execute(SchedulingCommand command)
		{
			var events = CreateEvents(command);
			_eventPublisher.Publish(events.ToArray());
		}

		[ReadonlyUnitOfWork]
		protected virtual IEnumerable<SchedulingWasOrdered> CreateEvents(SchedulingCommand command)
		{
			var events = new List<SchedulingWasOrdered>();
			using (CommandScope.Create(command))
			{
				var userLocks = _gridLockManager.LockInfos();
				var islands = _createIslands.Create(command.Period);
				var useTeam = _schedulingUseTeamProvider.Fetch(command.PlanningPeriodId);
				if (useTeam)
				{
					var agentsAndSkills = _crossAgentsAndSkills.Execute(islands, command.AgentsToSchedule);
					addEvent(events, command, command.AgentsToSchedule, agentsAndSkills.Agents.ToHashSet(),
						agentsAndSkills.Skills, userLocks);
				}
				else
				{
					foreach (var island in islands)
					{
						var agentsInIslandIds = island.AgentsInIsland().Select(x => x.Id.Value).ToHashSet();
						addEvent(events, command, command.AgentsToSchedule, agentsInIslandIds, island.SkillIds(), userLocks);
					}
				}
			}

			return events;
		}

		private static void addEvent(ICollection<SchedulingWasOrdered> events, SchedulingCommand command, IEnumerable<IPerson> allAgentsToSchedule, 
			HashSet<Guid> agentsInIslandIds, IEnumerable<Guid> skillsInIslandsIds, IEnumerable<LockInfo> userLocks)
		{
			var agentsToScheduleInIsland = allAgentsToSchedule.Where(x => agentsInIslandIds.Contains(x.Id.Value)).ToArray();
			if (agentsToScheduleInIsland.Any())
			{
				events.Add(new SchedulingWasOrdered
				{
					Agents = agentsToScheduleInIsland.Select(x => x.Id.Value),
					AgentsInIsland = agentsInIslandIds,
					StartDate = command.Period.StartDate,
					EndDate = command.Period.EndDate,
					CommandId = command.CommandId,
					UserLocks = userLocks,
					Skills = skillsInIslandsIds,
					FromWeb = command.FromWeb,
					ScheduleWithoutPreferencesForFailedAgents = command.ScheduleWithoutPreferencesForFailedAgents,
					PlanningPeriodId = command.PlanningPeriodId,
					RunDayOffOptimization = command.RunDayOffOptimization
				});				
			}
		}
	}
}