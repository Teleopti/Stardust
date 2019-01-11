using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly CreateIslands _createIslands;
		private readonly IAllStaff _allStaff;
		private readonly IOptimizationPreferencesProvider _optimizationPreferencesProvider;
		private readonly CrossAgentsAndSkills _crossAgentsAndSkills;
		private readonly IGridlockManager _gridLockManager;

		public DayOffOptimizationCommandHandler(IEventPublisher eventPublisher,
			CreateIslands createIslands,
			IAllStaff allStaff,
			IOptimizationPreferencesProvider optimizationPreferencesProvider,
			CrossAgentsAndSkills crossAgentsAndSkills,
			IGridlockManager gridLockManager)
		{
			_eventPublisher = eventPublisher;
			_createIslands = createIslands;
			_allStaff = allStaff;
			_optimizationPreferencesProvider = optimizationPreferencesProvider;
			_crossAgentsAndSkills = crossAgentsAndSkills;
			_gridLockManager = gridLockManager;
		}
		
		[TestLog]
		public virtual void Execute(DayOffOptimizationCommand command)
		{
			var islands = CreateIslands(command.Period, command);
			var evts = new List<DayOffOptimizationWasOrdered>();
			if (teamScheduling(command))
			{
				var agentsToSchedule = command.AgentsToOptimize ?? AllAgents_DeleteThisLater(command).Where(x => !x.IsExternalAgent);
				var agentIds = agentsToSchedule.Select(x => x.Id.Value);
				var crossAgentsAndSkillsResult = _crossAgentsAndSkills.Execute(islands, agentsToSchedule);
				evts.Add(new DayOffOptimizationWasOrdered
				{
					StartDate = command.Period.StartDate,
					EndDate = command.Period.EndDate,
					Agents = agentIds,
					AgentsInIsland = crossAgentsAndSkillsResult.Agents,
					Skills = crossAgentsAndSkillsResult.Skills,
					CommandId = command.CommandId,
					UserLocks = _gridLockManager.LockInfos()
				});
			}
			else
			{
				foreach (var island in islands)
				{
					var agentsInIsland = island.AgentsInIsland().ToArray();
					var agentsToOptimize = command.AgentsToOptimize?.Intersect(agentsInIsland).ToArray() ?? agentsInIsland;
					if (agentsToOptimize.Any())
					{
						evts.Add(new DayOffOptimizationWasOrdered
						{
							StartDate = command.Period.StartDate,
							EndDate = command.Period.EndDate,
							Agents = agentsToOptimize.Select(x=>x.Id.Value),
							AgentsInIsland = agentsInIsland.Select(x=>x.Id.Value),
							Skills = island.SkillIds(),
							CommandId = command.CommandId,
							UserLocks = _gridLockManager.LockInfos()
						});	
					}
				}
			}

			_eventPublisher.Publish(evts.ToArray());
		}
		
		
		//REMOVE ME WHEN TEAM + ISLANDS WORKS
		[UnitOfWork]
		public virtual IEnumerable<IPerson> AllAgents_DeleteThisLater(DayOffOptimizationCommand command)
		{
			return _allStaff.Agents(command.Period);
		}
		private bool teamScheduling(DayOffOptimizationCommand command)
		{
			using (CommandScope.Create(command))
			{
				return _optimizationPreferencesProvider.Fetch().Extra.UseTeams;
			}
		}
		//
		
		[UnitOfWork]
		protected virtual IEnumerable<Island> CreateIslands(DateOnlyPeriod period, DayOffOptimizationCommand command)
		{
			using (CommandScope.Create(command))
			{
				return _createIslands.Create(period);
			}
		}
	}
}