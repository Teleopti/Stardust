using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationCommandHandler
	{
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly IGridlockManager _gridLockManager;
		private readonly IAllStaff _allStaff;
		private readonly IOptimizationPreferencesProvider _optimizationPreferencesProvider;
		private readonly CrossAgentsAndSkills _crossAgentsAndSkills;
		private readonly CreateIslands _createIslands;

		protected IntradayOptimizationCommandHandler(IEventPopulatingPublisher eventPublisher,
												IGridlockManager gridLockManager,
												IAllStaff allStaff,
												IOptimizationPreferencesProvider optimizationPreferencesProvider,
												CrossAgentsAndSkills crossAgentsAndSkills,
												CreateIslands createIslands)
		{
			_eventPublisher = eventPublisher;
			_gridLockManager = gridLockManager;
			_allStaff = allStaff;
			_optimizationPreferencesProvider = optimizationPreferencesProvider;
			_crossAgentsAndSkills = crossAgentsAndSkills;
			_createIslands = createIslands;
		}

		public void Execute(IntradayOptimizationCommand command)
		{
			var islands = CreateIslands(command.Period, command);
			var events = createOptimizationWasOrderedEvents(command, islands, _gridLockManager.LockInfos());
			_eventPublisher.Publish(events.ToArray());
		}

		private IEnumerable<IEvent> createOptimizationWasOrderedEvents(IntradayOptimizationCommand command, IEnumerable<Island> islands, IEnumerable<LockInfo> lockInfos)
		{
			var events = new List<IntradayOptimizationWasOrdered>();

			if (teamScheduling(command))
			{
				var agentsToSchedule = command.AgentsToOptimize ?? AllAgents_DeleteThisLater(command).Where(x => !x.IsExternalAgent);
				var agentIds = agentsToSchedule.Select(x => x.Id.Value);
				var crossAgentsAndSkillsResult = _crossAgentsAndSkills.Execute(islands, agentsToSchedule);
				events.Add(new IntradayOptimizationWasOrdered
				{
					StartDate = command.Period.StartDate,
					EndDate = command.Period.EndDate,
					RunResolveWeeklyRestRule = command.RunResolveWeeklyRestRule,
					AgentsInIsland = crossAgentsAndSkillsResult.Agents,
					Agents = agentIds,
					CommandId = command.CommandId,
					UserLocks = lockInfos,
					Skills = crossAgentsAndSkillsResult.Skills
				});
			}
			else
			{
				foreach (var island in islands)
				{
					var agentsInIsland = island.AgentsInIsland();
					var agentsToOptimize = command.AgentsToOptimize?.Where(x => agentsInIsland.Contains(x)).ToArray()
										   ?? agentsInIsland;

					if (agentsToOptimize.Any())
					{
						events.Add(new IntradayOptimizationWasOrdered
						{
							StartDate = command.Period.StartDate,
							EndDate = command.Period.EndDate,
							RunResolveWeeklyRestRule = command.RunResolveWeeklyRestRule,
							AgentsInIsland = agentsInIsland.Select(x => x.Id.Value),
							Agents = agentsToOptimize.Select(x => x.Id.Value),
							CommandId = command.CommandId,
							UserLocks = lockInfos,
							Skills = island.SkillIds(),
							PlanningPeriodId = command.PlanningPeriodId
						});
					}
				}
			}
			return events;
		}

		[UnitOfWork]
		protected virtual IEnumerable<Island> CreateIslands(DateOnlyPeriod period, IntradayOptimizationCommand command)
		{
			using (CommandScope.Create(command))
			{
				return _createIslands.Create(period);
			}
		}

		//REMOVE ME WHEN TEAM + ISLANDS WORKS
		[UnitOfWork]
		public virtual IEnumerable<IPerson> AllAgents_DeleteThisLater(IntradayOptimizationCommand command)
		{
			return _allStaff.Agents(command.Period);
		}
		private bool teamScheduling(IntradayOptimizationCommand command)
		{
			using (CommandScope.Create(command))
			{
				return _optimizationPreferencesProvider.Fetch().Extra.UseTeams;
			}
		}
		//
	}
}