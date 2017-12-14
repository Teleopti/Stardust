using System;
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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationCommandHandler : IDayOffOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly FetchIslands _fetchIslands;
		private readonly IAllStaff _allStaff;
		private readonly IOptimizationPreferencesProvider _optimizationPreferencesProvider;
		private readonly CrossAgentsAndSkills _crossAgentsAndSkills;
		private readonly IGridlockManager _gridLockManager;

		public DayOffOptimizationCommandHandler(IEventPublisher eventPublisher,
			FetchIslands fetchIslands,
			IAllStaff allStaff,
			IOptimizationPreferencesProvider optimizationPreferencesProvider,
			CrossAgentsAndSkills crossAgentsAndSkills,
			IGridlockManager gridLockManager)
		{
			_eventPublisher = eventPublisher;
			_fetchIslands = fetchIslands;
			_allStaff = allStaff;
			_optimizationPreferencesProvider = optimizationPreferencesProvider;
			_crossAgentsAndSkills = crossAgentsAndSkills;
			_gridLockManager = gridLockManager;
		}
		
		public void Execute(DayOffOptimizationCommand command, Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
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
					RunWeeklyRestSolver = command.RunWeeklyRestSolver,
					PlanningPeriodId = command.PlanningPeriodId,
					CommandId = command.CommandId,
					UserLocks = _gridLockManager.LockInfos()
				});
			}
			else
			{
				foreach (var island in islands)
				{
					var agentsInIsland = island.AgentsInIsland().ToArray();
					var agentsToOptimize = command.AgentsToOptimize?.Where(x => agentsInIsland.Contains(x)).ToArray() ?? agentsInIsland;
					if (agentsToOptimize.Any())
					{
						evts.Add(new DayOffOptimizationWasOrdered
						{
							StartDate = command.Period.StartDate,
							EndDate = command.Period.EndDate,
							Agents = agentsToOptimize.Select(x=>x.Id.Value),
							AgentsInIsland = agentsInIsland.Select(x=>x.Id.Value),
							Skills = island.SkillIds(),
							RunWeeklyRestSolver = command.RunWeeklyRestSolver,
							PlanningPeriodId = command.PlanningPeriodId,
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
				return _fetchIslands.Execute(period);
			}
		}
	}
}