using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
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

		//REMOVE ME WHEN SCHEDULING + ISLANDS WORKS
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;
		//

		public SchedulingCommandHandler(IEventPublisher eventPublisher, 
				IGridlockManager gridLockManager,
				IAllStaff allStaff,
				ISchedulingOptionsProvider schedulingOptionsProvider,
				CrossAgentsAndSkills crossAgentsAndSkills,
				CreateIslands createIslands,
				IExcludeAgentsWithHints excludeAgentsWithHints)
		{
			_eventPublisher = eventPublisher;
			_gridLockManager = gridLockManager;
			_allStaff = allStaff;
			_schedulingOptionsProvider = schedulingOptionsProvider;
			_crossAgentsAndSkills = crossAgentsAndSkills;
			_createIslands = createIslands;
			_excludeAgentsWithHints = excludeAgentsWithHints;
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

				events.Add(createEvent(command, agentsToSchedule, agentsAndSkills.Agents, agentsAndSkills.Skills, userLocks));
			}
			else
			{
				foreach (var island in islands)
				{
					var agentsInIslands = island.AgentsInIsland().ToArray();
					var agentsToSchedule = command.AgentsToSchedule?.Where(x => agentsInIslands.Contains(x)).ToArray() ?? agentsInIslands;

					if (agentsToSchedule.Any())
					{
						var @event = createEvent(command, agentsToSchedule, agentsInIslands.Select(x => x.Id.Value), island.SkillIds(), userLocks);
						events.Add(@event);
					}
				}
			}

			_eventPublisher.Publish(events.ToArray());
		}

		private SchedulingWasOrdered createEvent(SchedulingCommand command, IEnumerable<IPerson> agentsToSchedule, IEnumerable<Guid> agentsInIslandsIds, 
			IEnumerable<Guid> skillIds, IEnumerable<LockInfo> userLocks)
		{
			var filteredAgentsToSchedule = _excludeAgentsWithHints.Execute(agentsToSchedule, command.Period, null).Select(x => x.Id.Value);
			return new SchedulingWasOrdered
			{
				Agents = filteredAgentsToSchedule,
				AgentsInIsland = agentsInIslandsIds,
				StartDate = command.Period.StartDate,
				EndDate = command.Period.EndDate,
				CommandId = command.CommandId,
				UserLocks = userLocks,
				Skills = skillIds,
				FromWeb = command.FromWeb,
				ScheduleWithoutPreferencesForFailedAgents = command.ScheduleWithoutPreferencesForFailedAgents,
				PlanningPeriodId = command.PlanningPeriodId,
				RunDayOffOptimization = command.RunDayOffOptimization
			};
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