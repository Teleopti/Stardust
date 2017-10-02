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
		private readonly CreateIslands _createIslands;
		private readonly ReduceSkillSets _reduceSkillSets;
		private readonly IPeopleInOrganization _peopleInOrganization;
		//REMOVE ME WHEN SCHEDULING + ISLANDS WORKS
		private readonly ISchedulingOptionsProvider _schedulingOptionsProvider;
		//

		public SchedulingCommandHandler(IEventPublisher eventPublisher, 
				IGridlockManager gridLockManager, 
				CreateIslands createIslands, 
				ReduceSkillSets reduceSkillSets, 
				IPeopleInOrganization peopleInOrganization,
				ISchedulingOptionsProvider schedulingOptionsProvider)
		{
			_eventPublisher = eventPublisher;
			_gridLockManager = gridLockManager;
			_createIslands = createIslands;
			_reduceSkillSets = reduceSkillSets;
			_peopleInOrganization = peopleInOrganization;
			_schedulingOptionsProvider = schedulingOptionsProvider;
		}

		[TestLog]
		public virtual void Execute(SchedulingCommand command)
		{
			var userLocks = _gridLockManager.LockInfos();
			var events = new List<SchedulingWasOrdered>();
			if (teamScheduling(command))
			{
				var agentIds = command.AgentsToSchedule ?? AllAgents_DeleteThisLater(command).Select(x=>x.Id.Value);
				events.Add(new SchedulingWasOrdered
				{
					AgentsToSchedule = agentIds,
					AgentsInIsland = agentIds,
					StartDate = command.Period.StartDate,
					EndDate = command.Period.EndDate,
					RunWeeklyRestSolver = command.RunWeeklyRestSolver,
					CommandId = command.CommandId,
					UserLocks = userLocks,
					FromWeb= command.FromWeb,
					PlanningPeriodId = command.PlanningPeriodId
				});
			}
			else
			{
				var islands = CreateIslands(command.Period, command);
				foreach (var island in islands)
				{
					var agentsInIsland = island.AgentsInIsland().Select(x => x.Id.Value).ToArray();
					var agentsToSchedule = command.AgentsToSchedule?.Where(x => agentsInIsland.Contains(x)).ToArray() ?? agentsInIsland;

					if (agentsToSchedule.Any())
					{
						var @event = new SchedulingWasOrdered
						{
							AgentsToSchedule = agentsToSchedule,
							AgentsInIsland = agentsInIsland,
							StartDate = command.Period.StartDate,
							EndDate = command.Period.EndDate,
							RunWeeklyRestSolver = command.RunWeeklyRestSolver,
							CommandId = command.CommandId,
							UserLocks = userLocks,
							Skills = island.SkillIds(),
							FromWeb = command.FromWeb,
							PlanningPeriodId = command.PlanningPeriodId
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
			return _peopleInOrganization.Agents(command.Period);
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
				return _createIslands.Create(_reduceSkillSets, _peopleInOrganization.Agents(period), period);
			}
		}
	}
}