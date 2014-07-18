using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class AgentBadgeCalculationConsumer : ConsumerOf<AgentBadgeCalculateMessage>
	{
		private readonly IServiceBus _serviceBus;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IEnumerable<IAgentBadgeCalculator> _calculators;

		public AgentBadgeCalculationConsumer(IServiceBus serviceBus, IRepositoryFactory repositoryFactory)
		{
			_serviceBus = serviceBus;
			_repositoryFactory = repositoryFactory;
			if (_repositoryFactory != null)
			{
				_calculators = new IAgentBadgeCalculator[]
				{
					new AgentBadgeAnsweredCallsCalculator(_repositoryFactory.CreateStatisticRepository()),
					new AgentBadgeAdherenceCalculator(_repositoryFactory.CreateStatisticRepository()),
					new AgentBadgeAHTCalculator(_repositoryFactory.CreateStatisticRepository()),
				};
			}
		}

		public void Consume(AgentBadgeCalculateMessage message)
		{
			foreach (var dataSource in GetRegisteredDataSourceCollection().Where(dataSource => dataSource.Statistic != null && dataSource.Application != null))
			{
				IList<IPerson> allAgents;
				using (var uow = dataSource.Application.CurrentUnitOfWork())
				{
					allAgents = _repositoryFactory.CreatePersonRepository(uow).LoadAll();
				}
				using (var uow = dataSource.Statistic.CurrentUnitOfWork())
				{
					foreach (var calculator in _calculators)
					{
						calculator.Calculate(uow, allAgents);
					}
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
}