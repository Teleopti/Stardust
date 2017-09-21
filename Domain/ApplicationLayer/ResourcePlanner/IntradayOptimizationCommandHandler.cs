using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly CreateIslands _createIslands;
		private readonly IGridlockManager _gridLockManager;
		private readonly ReduceSkillSets _reduceSkillSets;
		private readonly IPeopleInOrganization _peopleInOrganization;
		private readonly IOptimizationPreferencesProvider _optimizationPreferencesProvider;

		protected IntradayOptimizationCommandHandler(IEventPublisher eventPublisher,
												CreateIslands createIslands,
												IGridlockManager gridLockManager,
												ReduceSkillSets reduceSkillSets,
												IPeopleInOrganization peopleInOrganization,
												IOptimizationPreferencesProvider optimizationPreferencesProvider)
		{
			_eventPublisher = eventPublisher;
			_createIslands = createIslands;
			_gridLockManager = gridLockManager;
			_reduceSkillSets = reduceSkillSets;
			_peopleInOrganization = peopleInOrganization;
			_optimizationPreferencesProvider = optimizationPreferencesProvider;
		}

		public void Execute(IntradayOptimizationCommand command)
		{
			var islands = CreateIslands(command.Period, command);
			var events = command.RunAsynchronously
				? createWebIntradayOptimizationStardustEvents(command, islands, _gridLockManager.LockInfos())
				: createOptimizationWasOrderedEvents(command, islands, _gridLockManager.LockInfos());
			_eventPublisher.Publish(events.ToArray());
		}

		private IEnumerable<IEvent> createOptimizationWasOrderedEvents(IntradayOptimizationCommand command, IEnumerable<Island> islands, IEnumerable<LockInfo> lockInfos)
		{
			var events = new List<IntradayOptimizationWasOrdered>();

			if (teamScheduling(command))
			{
				var agentsToSchedule = command.AgentsToOptimize ?? AllAgents_DeleteThisLater(command);
				var agentIds = agentsToSchedule.Select(x => x.Id.Value);
				events.Add(new IntradayOptimizationWasOrdered
				{
					StartDate = command.Period.StartDate,
					EndDate = command.Period.EndDate,
					RunResolveWeeklyRestRule = command.RunResolveWeeklyRestRule,
					AgentsInIsland = agentIds,
					AgentsToOptimize = agentIds,
					CommandId = command.CommandId,
					UserLocks = lockInfos
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
							AgentsToOptimize = agentsToOptimize.Select(x => x.Id.Value),
							CommandId = command.CommandId,
							UserLocks = lockInfos,
							Skills = island.SkillIds()
						});
					}
				}
			}
			return events;
		}

		private IEnumerable<IEvent> createWebIntradayOptimizationStardustEvents(IntradayOptimizationCommand command, IEnumerable<Island> islands, IEnumerable<LockInfo> lockInfos)
		{
			var orgEvents = createOptimizationWasOrderedEvents(command, islands, lockInfos);
			var stardustEvents = orgEvents.Select(x => new WebIntradayOptimizationStardustEvent { IntradayOptimizationWasOrdered = (IntradayOptimizationWasOrdered)x }).ToArray();
			var numberOfEvents = stardustEvents.Length;
			foreach (var stardustEvent in stardustEvents)
			{
				stardustEvent.TotalEvents = numberOfEvents;
				if (command.JobResultId.HasValue)
					stardustEvent.JobResultId = command.JobResultId.Value;
				if (command.PlanningPeriodId.HasValue)
					stardustEvent.PlanningPeriodId = command.PlanningPeriodId.Value;
			}
			return stardustEvents;
		}

		[UnitOfWork]
		protected virtual IEnumerable<Island> CreateIslands(DateOnlyPeriod period, IntradayOptimizationCommand command)
		{
			using (CommandScope.Create(command))
			{
				return _createIslands.Create(_reduceSkillSets, _peopleInOrganization.Agents(period), period);
			}
		}

		//REMOVE ME WHEN TEAM + ISLANDS WORKS
		[UnitOfWork]
		public virtual IEnumerable<IPerson> AllAgents_DeleteThisLater(IntradayOptimizationCommand command)
		{
			return _peopleInOrganization.Agents(command.Period);
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