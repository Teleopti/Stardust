using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class AgentBadgeCalculationConsumer : ConsumerOf<AgentBadgeCalculateMessage>
	{
		private readonly IServiceBus _serviceBus;
		private readonly IRepositoryFactory _repositoryFactory;

		public AgentBadgeCalculationConsumer(IServiceBus serviceBus, IRepositoryFactory repositoryFactory)
		{
			_serviceBus = serviceBus;
			_repositoryFactory = repositoryFactory;
		}

		public void Consume(AgentBadgeCalculateMessage message)
		{
			foreach (var dataSource in GetRegisteredDataSourceCollection())
			{
				if (dataSource.Statistic == null || dataSource.Application == null) continue;
				IEnumerable<Tuple<Guid, int>> agentsThatShouldGetBadge;
				using (var uow = dataSource.Statistic.CurrentUnitOfWork())
				{
					agentsThatShouldGetBadge = _repositoryFactory.CreateStatisticRepository().LoadAgentsOverThresholdForAnsweredCalls(uow);
				}
				IList<IPerson> allAgents;
				using (var uow = dataSource.Application.CurrentUnitOfWork())
				{
					allAgents = _repositoryFactory.CreatePersonRepository(uow).LoadAll();
				}

				foreach (var a in agentsThatShouldGetBadge.Select(agent => allAgents.Single(x => x.Id.Value == agent.Item1)).Where(a => a != null))
				{
					a.AddBadge(new Domain.Common.AgentBadge { BronzeBadge = 1 });
				}
			}

			if (_serviceBus != null)
			{
				_serviceBus.DelaySend(DateTime.Now.AddDays(1), message);
			}
		}

		protected virtual IEnumerable<IDataSource> GetRegisteredDataSourceCollection()
		{
			return StateHolderReader.Instance.StateReader.ApplicationScopeData.RegisteredDataSourceCollection;
		}
	}

	public class AgentBadgeCalculateMessage : RaptorDomainEvent
	{

	}
}