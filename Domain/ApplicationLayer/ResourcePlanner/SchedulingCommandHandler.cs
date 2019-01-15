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
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;

		public SchedulingCommandHandler(IEventPublisher eventPublisher, 
				IGridlockManager gridLockManager,
				ISchedulingOptionsProvider schedulingOptionsProvider,
				CrossAgentsAndSkills crossAgentsAndSkills,
				CreateIslands createIslands)
		{
			_eventPublisher = eventPublisher;
			_gridLockManager = gridLockManager;
			_schedulingOptionsProvider = schedulingOptionsProvider;
			_crossAgentsAndSkills = crossAgentsAndSkills;
			_createIslands = createIslands;
		}

		[TestLog]
		public virtual void Execute(SchedulingCommand command)
		{
			var events = new List<SchedulingWasOrdered>();
			using (CommandScope.Create(command))
			{
				var userLocks = _gridLockManager.LockInfos();
				var islands = CreateIslands(command.Period, command);
				if (_schedulingOptionsProvider.Fetch(null).UseTeam)
				{
					var agentsAndSkills = _crossAgentsAndSkills.Execute(islands, command.AgentsToSchedule);
					addEvent(events, command, command.AgentsToSchedule, agentsAndSkills.Agents.ToHashSet(), agentsAndSkills.Skills, userLocks);
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
			_eventPublisher.Publish(events.ToArray());
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

		
		[ReadonlyUnitOfWork]
		protected virtual IEnumerable<Island> CreateIslands(DateOnlyPeriod period, SchedulingCommand command)
		{
			return _createIslands.Create(period);
		}
	}
}