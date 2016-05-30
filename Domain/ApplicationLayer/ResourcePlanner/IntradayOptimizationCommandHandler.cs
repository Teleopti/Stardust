using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationCommandHandler : IIntradayOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly CreateIslands _createIslands;
		private readonly IGridlockManager _gridLockManager;

		public IntradayOptimizationCommandHandler(IEventPublisher eventPublisher, CreateIslands createIslands, IGridlockManager gridLockManager) //TODO - will it work without Func?
		{
			_eventPublisher = eventPublisher;
			_createIslands = createIslands;
			_gridLockManager = gridLockManager;
		}

		public void Execute(IntradayOptimizationCommand command)
		{
			var islands = CreateIslands(command.Period, command);
			var events = new List<OptimizationWasOrdered>();

			foreach (var island in islands)
			{
				var agentsInIsland = island.PersonsInIsland();
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
						UserLocks = _gridLockManager.LockInfos()
					});
				}
			}
			_eventPublisher.Publish(events.ToArray());
		}

		[UnitOfWork]
		protected virtual IEnumerable<Island> CreateIslands(DateOnlyPeriod period, IntradayOptimizationCommand command)
		{
			using (CommandScope.Create(command))
			{
				return _createIslands.Create(period);
			}
		}
	}

	//remove below when toggle 36939 is removed
	public interface IIntradayOptimizationCommandHandler
	{
		void Execute(IntradayOptimizationCommand command);
	}
	public class IntradayOptimizationOneThreadCommandHandler : IIntradayOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly IPeopleInOrganization _peopleInOrganization;
		private readonly IGridlockManager _gridLockManager;

		public IntradayOptimizationOneThreadCommandHandler(IEventPublisher eventPublisher, IPeopleInOrganization peopleInOrganization, IGridlockManager gridLockManager) //TODO - will it work without Func?
		{
			_eventPublisher = eventPublisher;
			_peopleInOrganization = peopleInOrganization;
			_gridLockManager = gridLockManager;
		}

		public void Execute(IntradayOptimizationCommand command)
		{
			var agentsToOptimize = command.AgentsToOptimize == null ?
					LoadPeopleInOrganization(command.Period).Select(x => x.Id.Value) :
					command.AgentsToOptimize.Select(x => x.Id.Value);

			_eventPublisher.Publish(new OptimizationWasOrdered
			{
				Period = command.Period,
				AgentsInIsland = LoadPeopleInOrganization(command.Period).Select(x => x.Id.Value),
				AgentsToOptimize = agentsToOptimize,
				RunResolveWeeklyRestRule = command.RunResolveWeeklyRestRule,
				CommandId = command.CommandId,
				UserLocks = _gridLockManager.LockInfos()
			});
		}

		[UnitOfWork]
		protected virtual IEnumerable<IPerson> LoadPeopleInOrganization(DateOnlyPeriod period)
		{
			return _peopleInOrganization.Agents(period);
		}
	}
}