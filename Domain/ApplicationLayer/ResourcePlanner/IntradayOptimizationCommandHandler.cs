using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
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
		private readonly ReduceSkillGroups _reduceSkillGroups;
		private readonly IPeopleInOrganization _peopleInOrganization;

		protected IntradayOptimizationCommandHandler(IEventPublisher eventPublisher,
												CreateIslands createIslands,
												IGridlockManager gridLockManager,
												ReduceSkillGroups reduceSkillGroups,
												IPeopleInOrganization peopleInOrganization)
		{
			_eventPublisher = eventPublisher;
			_createIslands = createIslands;
			_gridLockManager = gridLockManager;
			_reduceSkillGroups = reduceSkillGroups;
			_peopleInOrganization = peopleInOrganization;
		}

		public void Execute(IntradayOptimizationCommand command)
		{
			var islands = CreateIslands(command.Period, command);
			var events = command.RunAsynchronously
				? CreateWebIntradayOptimizationStardustEvents(command, islands, _gridLockManager.LockInfos())
				: CreateOptimizationWasOrderedEvents(command, islands, _gridLockManager.LockInfos());
			_eventPublisher.Publish(events.ToArray());
		}


		private IEnumerable<IEvent> CreateOptimizationWasOrderedEvents(IntradayOptimizationCommand command, IEnumerable<Island> islands, IEnumerable<LockInfo> lockInfos)
		{
			var events = new List<IntradayOptimizationWasOrdered>();

			foreach (var island in islands)
			{
				var agentsInIsland = island.AgentsInIsland();
				var agentsToOptimize = command.AgentsToOptimize?.Where(x => agentsInIsland.Contains(x)).ToArray()
									   ?? agentsInIsland;

				if (agentsToOptimize.Any())
				{
					var optimizationWasOrdered = createOptimizationWasOrderedEvent(command, lockInfos, agentsInIsland, agentsToOptimize, island);
					events.Add(optimizationWasOrdered);
				}
			}
			return events;
		}

		private IEnumerable<IEvent> CreateWebIntradayOptimizationStardustEvents(IntradayOptimizationCommand command, IEnumerable<Island> islands, IEnumerable<LockInfo> lockInfos)
		{
			var events = new List<WebIntradayOptimizationStardustEvent>();

			foreach (var island in islands)
			{
				var agentsInIsland = island.AgentsInIsland();
				var agentsToOptimize = command.AgentsToOptimize?.Where(x => agentsInIsland.Contains(x)).ToArray()
									   ?? agentsInIsland;

				if (agentsToOptimize.Any())
				{
					var optimizationWasOrdered = createOptimizationWasOrderedEvent(command, lockInfos, agentsInIsland, agentsToOptimize, island);
					events.Add(new WebIntradayOptimizationStardustEvent
					{
						IntradayOptimizationWasOrdered = optimizationWasOrdered
					});
				}
			}
			events.ForEach(e => e.TotalEvents = events.Count);
			if (command.JobResultId != null)
				events.ForEach(e => e.JobResultId = command.JobResultId.Value);
			if (command.PlanningPeriodId != null)
				events.ForEach(e => e.PlanningPeriodId = command.PlanningPeriodId.Value);
			return events;
		}

		private static IntradayOptimizationWasOrdered createOptimizationWasOrderedEvent(IntradayOptimizationCommand command, IEnumerable<LockInfo> lockInfos, IEnumerable<IPerson> agentsInIsland, IEnumerable<IPerson> agentsToOptimize, Island island)
		{
			return new IntradayOptimizationWasOrdered
			{
				StartDate = command.Period.StartDate,
				EndDate = command.Period.EndDate,
				RunResolveWeeklyRestRule = command.RunResolveWeeklyRestRule,
				AgentsInIsland = agentsInIsland.Select(x => x.Id.Value),
				AgentsToOptimize = agentsToOptimize.Select(x => x.Id.Value),
				CommandId = command.CommandId,
				UserLocks = lockInfos,
				Skills = island.SkillIds()
			};
		}

		[UnitOfWork]
		protected virtual IEnumerable<Island> CreateIslands(DateOnlyPeriod period, IntradayOptimizationCommand command)
		{
			using (CommandScope.Create(command))
			{
				return _createIslands.Create(_reduceSkillGroups, _peopleInOrganization.Agents(period), period);
			}
		}
	}
}