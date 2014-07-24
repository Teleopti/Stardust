using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class AgentBadgeCalculationConsumer : ConsumerOf<AgentBadgeCalculateMessage>
	{
		private readonly IServiceBus _serviceBus;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IAgentBadgeCalculator _calculator;

		public AgentBadgeCalculationConsumer(IServiceBus serviceBus, IRepositoryFactory repositoryFactory)
		{
			_serviceBus = serviceBus;
			_repositoryFactory = repositoryFactory;
			if (_repositoryFactory != null)
			{
				_calculator = new AgentBadgeCalculator(_repositoryFactory.CreateStatisticRepository());
			}
		}

		public void Consume(AgentBadgeCalculateMessage message)
		{
			foreach (var dataSource in GetRegisteredDataSourceCollection().Where(dataSource => dataSource.Statistic != null && dataSource.Application != null))
			{
				AdherenceReportSettingCalculationMethod adherenceCalculationMethod;
				IList<IPerson> allAgents;
				using (var uow = dataSource.Application.CurrentUnitOfWork())
				{
					adherenceCalculationMethod = new GlobalSettingDataRepository(uow).FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting()).CalculationMethod;
					allAgents = _repositoryFactory.CreatePersonRepository(uow).LoadAll();
				}
				using (var uow = dataSource.Statistic.CurrentUnitOfWork())
				{
					_calculator.Calculate(uow, allAgents, adherenceCalculationMethod);
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