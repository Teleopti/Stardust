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
	public class SchedulingCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly IGridlockManager _gridLockManager;
		private readonly IAllStaff _allStaff;
		private readonly CrossAgentsAndSkills _crossAgentsAndSkills;
		private readonly CreateIslands _createIslands;

		//REMOVE ME WHEN SCHEDULING + ISLANDS WORKS
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;
		//

		public SchedulingCommandHandler(IEventPublisher eventPublisher, 
				IGridlockManager gridLockManager,
				IAllStaff allStaff,
				ISchedulingOptionsProvider schedulingOptionsProvider,
				CrossAgentsAndSkills crossAgentsAndSkills,
				CreateIslands createIslands)
		{
			_eventPublisher = eventPublisher;
			_gridLockManager = gridLockManager;
			_allStaff = allStaff;
			_schedulingOptionsProvider = schedulingOptionsProvider;
			_crossAgentsAndSkills = crossAgentsAndSkills;
			_createIslands = createIslands;
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
				events.Add(new SchedulingWasOrdered
				{
					Agents = agentsToSchedule.Select(x => x.Id.Value),
					AgentsInIsland = agentsAndSkills.Agents,
					StartDate = command.Period.StartDate,
					EndDate = command.Period.EndDate,
					CommandId = command.CommandId,
					UserLocks = userLocks,
					FromWeb= command.FromWeb,
					PlanningPeriodId = command.PlanningPeriodId,
					Skills = agentsAndSkills.Skills,
					RunDayOffOptimization = command.RunDayOffOptimization
				});
			}
			else
			{
				foreach (var island in islands)
				{
					var agentsInIsland = island.AgentsInIsland().ToArray();
					var agentsToSchedule = command.AgentsToSchedule?.Where(x => agentsInIsland.Contains(x)).ToArray() ?? agentsInIsland;

					if (agentsToSchedule.Any())
					{
						var @event = new SchedulingWasOrdered
						{
							Agents = agentsToSchedule.Select(x=>x.Id.Value),
							AgentsInIsland = agentsInIsland.Select(x=>x.Id.Value),
							StartDate = command.Period.StartDate,
							EndDate = command.Period.EndDate,
							CommandId = command.CommandId,
							UserLocks = userLocks,
							Skills = island.SkillIds(),
							FromWeb = command.FromWeb,
							PlanningPeriodId = command.PlanningPeriodId,
							RunDayOffOptimization = command.RunDayOffOptimization
						};
						events.Add(@event);
					}
				}
			}

			_eventPublisher.Publish(events.ToArray());
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