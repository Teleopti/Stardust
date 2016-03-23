using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner
{
	public class IntradayOptimizationCommandHandler : IIntradayOptimizationCommandHandler
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly CreateIslands _createIslands;

		public IntradayOptimizationCommandHandler(IEventPublisher eventPublisher, CreateIslands createIslands)
		{
			_eventPublisher = eventPublisher;
			_createIslands = createIslands;
		}

		public void Execute(IntradayOptimizationCommand command)
		{
			_eventPublisher.Publish(CreateIslands(command.Period).Select(island => new OptimizationWasOrdered
			{
				Period = command.Period,
				AgentIds = island.PersonsInIsland().Select(x => x.Id.Value),
				RunResolveWeeklyRestRule = command.RunResolveWeeklyRestRule
			}).ToArray());
		}

		[UnitOfWork]
		protected virtual IEnumerable<Island> CreateIslands(DateOnlyPeriod period)
		{
			return _createIslands.Create(period);
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

		public IntradayOptimizationOneThreadCommandHandler(IEventPublisher eventPublisher, IPeopleInOrganization peopleInOrganization)
		{
			_eventPublisher = eventPublisher;
			_peopleInOrganization = peopleInOrganization;
		}

		public void Execute(IntradayOptimizationCommand command)
		{
			_eventPublisher.Publish(new OptimizationWasOrdered
			{
				Period = command.Period,
				AgentIds = LoadPeopleInOrganization(command.Period).Select(x => x.Id.Value),
				RunResolveWeeklyRestRule = command.RunResolveWeeklyRestRule
			});
		}

		[UnitOfWork]
		protected virtual IEnumerable<IPerson> LoadPeopleInOrganization(DateOnlyPeriod period)
		{
			return _peopleInOrganization.Agents(period);
		}
	}
}