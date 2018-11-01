using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class SchedulingCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly IGridlockManager _gridLockManager;
		private readonly CrossAgentsAndSkills _crossAgentsAndSkills;
		private readonly CreateIslands _createIslands;
		private readonly IExcludeAgentsWithHints _excludeAgentsWithHints;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		//REMOVE ME WHEN SCHEDULING + ISLANDS WORKS
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;
		//

		public SchedulingCommandHandler(IEventPublisher eventPublisher, 
				IGridlockManager gridLockManager,
				ISchedulingOptionsProvider schedulingOptionsProvider,
				CrossAgentsAndSkills crossAgentsAndSkills,
				CreateIslands createIslands,
				IExcludeAgentsWithHints excludeAgentsWithHints,
				ICurrentUnitOfWork currentUnitOfWork)
		{
			_eventPublisher = eventPublisher;
			_gridLockManager = gridLockManager;
			_schedulingOptionsProvider = schedulingOptionsProvider;
			_crossAgentsAndSkills = crossAgentsAndSkills;
			_createIslands = createIslands;
			_excludeAgentsWithHints = excludeAgentsWithHints;
			_currentUnitOfWork = currentUnitOfWork;
		}

		[TestLog]
		public virtual void Execute(SchedulingCommand command)
		{
			var userLocks = _gridLockManager.LockInfos();
			var events = new List<SchedulingWasOrdered>();
			var islands = CreateIslands(command.Period, command);
			var allAgentsToSchedule = RemoveAgentsWithHints(command.AgentsToSchedule, command.Period);
			if (teamScheduling(command))
			{
				var agentsAndSkills = _crossAgentsAndSkills.Execute(islands, allAgentsToSchedule);
				addEvent(events, command, allAgentsToSchedule, agentsAndSkills.Agents, agentsAndSkills.Skills, userLocks);
			}
			else
			{
				foreach (var island in islands)
				{
					var agentsInIslands = island.AgentsInIsland().ToArray();
					addEvent(events, command, allAgentsToSchedule, agentsInIslands.Select(x => x.Id.Value), island.SkillIds(), userLocks);
				}
			}

			_eventPublisher.Publish(events.ToArray());
		}

		private static void addEvent(ICollection<SchedulingWasOrdered> events, SchedulingCommand command, IEnumerable<IPerson> allAgentsToSchedule, 
			IEnumerable<Guid> agentsInIslandsIds, IEnumerable<Guid> skillsInIslandsIds, IEnumerable<LockInfo> userLocks)
		{
			var agentsToScheduleInIsland = allAgentsToSchedule.Where(x => agentsInIslandsIds.Contains(x.Id.Value)).ToArray();
			if (agentsToScheduleInIsland.Any())
			{
				events.Add(new SchedulingWasOrdered
				{
					Agents = agentsToScheduleInIsland.Select(x => x.Id.Value),
					AgentsInIsland = agentsInIslandsIds,
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

		[UnitOfWork]
		[TestLog]
		protected virtual IEnumerable<IPerson> RemoveAgentsWithHints(IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			_currentUnitOfWork.Current().Reassociate(agents);
			return _excludeAgentsWithHints.Execute(agents, period, null);
		}

		private bool teamScheduling(SchedulingCommand command)
		{
			using (CommandScope.Create(command))
			{
				return _schedulingOptionsProvider.Fetch(null).UseTeam;
			}
		}

		[UnitOfWork]
		protected virtual IEnumerable<Island> CreateIslands(DateOnlyPeriod period, SchedulingCommand command)
		{
			using (CommandScope.Create(command))
			{
				return _createIslands.Create(period);
			}
		}
	}
}