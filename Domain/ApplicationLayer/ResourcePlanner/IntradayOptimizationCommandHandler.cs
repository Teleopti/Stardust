using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly ICreateIslands _createIslands;
		private readonly IGridlockManager _gridLockManager;
		private readonly ReduceSkillGroups _reduceSkillGroups;
		private readonly IPeopleInOrganization _peopleInOrganization;

		public IntradayOptimizationCommandHandler(IEventPublisher eventPublisher, 
												ICreateIslands createIslands, 
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
			var events = new List<OptimizationWasOrdered>();

			foreach (var island in islands)
			{
				var agentsInIsland = island.AgentsInIsland();
				var agentsToOptimize = command.AgentsToOptimize == null ? 
					agentsInIsland : 
					command.AgentsToOptimize.Where(x => agentsInIsland.Contains(x)).ToArray();

				if (agentsToOptimize.Any())
				{
					events.Add(new OptimizationWasOrdered
					{
						Period = command.Period,
						RunResolveWeeklyRestRule = command.RunResolveWeeklyRestRule,
						AgentsInIsland = agentsInIsland.Select(x => x.Id.Value),
						AgentsToOptimize = agentsToOptimize.Select(x => x.Id.Value),
						CommandId = command.CommandId,
						UserLocks = _gridLockManager.LockInfos(),
						Skills = island.SkillIds()
					});
				}
			}
			_eventPublisher.Publish(events.ToArray());
		}

		[UnitOfWork]
		protected virtual IEnumerable<IIsland> CreateIslands(DateOnlyPeriod period, IntradayOptimizationCommand command)
		{
			using (CommandScope.Create(command))
			{
				return _createIslands.Create(_reduceSkillGroups, _peopleInOrganization.Agents(period), period);
			}
		}
	}
}