﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationCommandHandler : IIntradayOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly CreateIslands _createIslands;
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public IntradayOptimizationCommandHandler(IEventPublisher eventPublisher, CreateIslands createIslands, ICurrentUnitOfWork currentUnitOfWork)
		{
			_eventPublisher = eventPublisher;
			_createIslands = createIslands;
			_currentUnitOfWork = currentUnitOfWork;
		}

		public void Execute(IntradayOptimizationCommand command)
		{
			_eventPublisher.Publish(Create(command).Select(island => new OptimizationWasOrdered
			{
				Period = command.Period,
				AgentIds = island.PersonsInIsland().Select(x => x.Id.Value),
				RunResolveWeeklyRestRule = command.RunResolveWeeklyRestRule
			}).ToArray());
		}

		[UnitOfWork]
		protected virtual IEnumerable<Island> Create(IntradayOptimizationCommand command)
		{
			//some hack to get rid of lazy load ex
			var uow = _currentUnitOfWork.Current();
			uow.Reassociate(command.Agents);
			//
			return _createIslands.Create(command.Period, command.Agents);
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

		public IntradayOptimizationOneThreadCommandHandler(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Execute(IntradayOptimizationCommand command)
		{
			_eventPublisher.Publish(new OptimizationWasOrdered
			{
				Period = command.Period,
				AgentIds = command.Agents.Select(x => x.Id.Value),
				RunResolveWeeklyRestRule = command.RunResolveWeeklyRestRule
			});
		}
	}
}