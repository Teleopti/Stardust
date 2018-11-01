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
		private readonly IAllStaff _allStaff;
		private readonly CrossAgentsAndSkills _crossAgentsAndSkills;
		private readonly CreateIslands _createIslands;
		private readonly IExcludeAgentsWithHints _excludeAgentsWithHints;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		//REMOVE ME WHEN SCHEDULING + ISLANDS WORKS
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;
		//

		public SchedulingCommandHandler(IEventPublisher eventPublisher, 
				IGridlockManager gridLockManager,
				IAllStaff allStaff,
				ISchedulingOptionsProvider schedulingOptionsProvider,
				CrossAgentsAndSkills crossAgentsAndSkills,
				CreateIslands createIslands,
				IExcludeAgentsWithHints excludeAgentsWithHints,
				ICurrentUnitOfWork currentUnitOfWork)
		{
			_eventPublisher = eventPublisher;
			_gridLockManager = gridLockManager;
			_allStaff = allStaff;
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
			if (teamScheduling(command))
			{
				var agentsToSchedule = command.AgentsToSchedule ?? AllAgents_DeleteThisLater(command).Where(x => !x.IsExternalAgent);
				var agentsAndSkills = _crossAgentsAndSkills.Execute(islands, agentsToSchedule);

				addEvent(events, command, agentsToSchedule, agentsAndSkills.Agents, agentsAndSkills.Skills, userLocks);
			}
			else
			{
				foreach (var island in islands)
				{
					var agentsInIslands = island.AgentsInIsland().ToArray();
					var agentsToSchedule = command.AgentsToSchedule?.Where(x => agentsInIslands.Contains(x)).ToArray() ?? agentsInIslands;

					if (agentsToSchedule.Any())
					{
						addEvent(events, command, agentsToSchedule, agentsInIslands.Select(x => x.Id.Value), island.SkillIds(), userLocks);
					}
				}
			}

			_eventPublisher.Publish(events.ToArray());
		}

		private void addEvent(ICollection<SchedulingWasOrdered> events, SchedulingCommand command, IEnumerable<IPerson> agentsToSchedule, 
			IEnumerable<Guid> agentsInIslandsIds, IEnumerable<Guid> skillsInIslandsIds, IEnumerable<LockInfo> userLocks)
		{
			var agentsToScheduleInIsland = agentsToSchedule.Where(x => agentsInIslandsIds.Contains(x.Id.Value));
			if (agentsToScheduleInIsland.Any())
			{
				var filteredAgentsToSchedule = RemoveAgentsWithHints(agentsToScheduleInIsland, command.Period).Select(x => x.Id.Value);
				events.Add(new SchedulingWasOrdered
				{
					Agents = filteredAgentsToSchedule,
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

		//REMOVE ME WHEN SCHEDULING + ISLANDS WORKS
		[UnitOfWork]
		public virtual IEnumerable<IPerson> AllAgents_DeleteThisLater(SchedulingCommand command)
		{
			return _allStaff.Agents(command.Period);
		}
		private bool teamScheduling(SchedulingCommand command)
		{
			using (CommandScope.Create(command))
			{
				return _schedulingOptionsProvider.Fetch(null).UseTeam;
			}
		}
		//

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