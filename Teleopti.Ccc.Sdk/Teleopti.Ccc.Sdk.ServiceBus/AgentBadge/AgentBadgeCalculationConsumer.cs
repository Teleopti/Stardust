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
				AdherenceReportSetting adherenceReportSetting;
				IList<IPerson> allAgents;
				using (var uow = dataSource.Application.CurrentUnitOfWork())
				{
					adherenceReportSetting = _repositoryFactory.CreateGlobalSettingDataRepository(uow).FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting());
					allAgents = _repositoryFactory.CreatePersonRepository(uow).LoadAll();
				}
				using (var uow = dataSource.Statistic.CurrentUnitOfWork())
				{
					var timeZoneList = _repositoryFactory.CreateStatisticRepository().LoadAllTimeZones(uow);
					if (message.IsInitialization)
					{
						foreach (var timezone in timeZoneList)
						{
							var todayForTimezone = DateTime.UtcNow.AddMinutes(timezone.Item3).Date;
							var yesterdayForTimezone = todayForTimezone.AddDays(-1);
							var tomorrowForTimezone = todayForTimezone.AddDays(1);

							_calculator.Calculate(uow, allAgents, timezone.Item1, yesterdayForTimezone, adherenceReportSetting.CalculationMethod);
							if (_serviceBus != null)
							{
								_serviceBus.DelaySend(tomorrowForTimezone, new AgentBadgeCalculateMessage
								{
									IsInitialization = false,
									TimezoneId = timezone.Item1
								});
							}
						}
					}
					else
					{
						var timezone = timeZoneList.First(tz => tz.Item1 == message.TimezoneId);
						if (timezone == null) continue;

						var todayForTimezone = DateTime.UtcNow.AddMinutes(timezone.Item3).Date;
						var yesterdayForTimezone = todayForTimezone.AddDays(-1);
						var tomorrowForTimezone = todayForTimezone.AddDays(1);

						_calculator.Calculate(uow, allAgents, timezone.Item1, yesterdayForTimezone, adherenceReportSetting.CalculationMethod);
						if (_serviceBus != null)
						{
							_serviceBus.DelaySend(tomorrowForTimezone, new AgentBadgeCalculateMessage
							{
								IsInitialization = false,
								TimezoneId = message.TimezoneId
							});
						}
					}
				}
			}
		}

		protected virtual IEnumerable<IDataSource> GetRegisteredDataSourceCollection()
		{
			return StateHolderReader.Instance.StateReader.ApplicationScopeData.RegisteredDataSourceCollection;
		}
	}
}