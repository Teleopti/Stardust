using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationCommandHandler : IDayOffOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly FetchIslands _fetchIslands;

		public DayOffOptimizationCommandHandler(IEventPublisher eventPublisher,
			FetchIslands fetchIslands)
		{
			_eventPublisher = eventPublisher;
			_fetchIslands = fetchIslands;
		}
		
		public void Execute(DayOffOptimizationCommand command, ISchedulingProgress schedulingProgress,
			Action<object, ResourceOptimizerProgressEventArgs> resourceOptimizerPersonOptimized)
		{
			var islands = CreateIslands(command.Period, command);
			var evts = new List<DayOffOptimizationWasOrdered>();
			foreach (var island in islands)
			{
				var agentsInIsland = island.AgentsInIsland().ToArray();
				var agentsToOptimize = command.AgentsToOptimize?.Where(x => agentsInIsland.Contains(x)).ToArray() ?? agentsInIsland;
				
				evts.Add(new DayOffOptimizationWasOrdered
				{
					StartDate = command.Period.StartDate,
					EndDate = command.Period.EndDate,
					Agents = agentsToOptimize.Select(x=>x.Id.Value),
					AgentsInIsland = agentsInIsland.Select(x=>x.Id.Value),
					Skills = island.SkillIds(),
					RunWeeklyRestSolver = command.RunWeeklyRestSolver,
					PlanningPeriodId = command.PlanningPeriodId,
					CommandId = command.CommandId
				});
			}

			_eventPublisher.Publish(evts.ToArray());
		}
		
		//TODO: reuse from schedulingcommandhandler (and intraday?)
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