using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
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
			_calculator.LastCalculatedDates = new Dictionary<int, DateTime>();
		}

		public void Consume(AgentBadgeCalculateMessage message)
		{
			foreach (var dataSource in GetRegisteredDataSourceCollection().Where(dataSource => dataSource.Statistic != null && dataSource.Application != null))
			{
				AdherenceReportSetting adherenceReportSetting;
				IList<IPerson> allAgents;
				using (var uow = dataSource.Application.CreateAndOpenUnitOfWork())
				{
					var agentBadgeSetting = _repositoryFactory.CreateAgentBadgeSettingsRepository(uow).LoadAll().FirstOrDefault();
					if (agentBadgeSetting == null || !agentBadgeSetting.EnableBadge)
					{
						// If no setting for agent badge or agent badge disabled
						// Then send message for next day to enable badge calculation (if the badge feature is enabled in this period).
						if (_serviceBus != null)
						{
							var nextCalculateDate = DateTime.Now.AddDays(1).Date;
							_serviceBus.DelaySend(nextCalculateDate, new AgentBadgeCalculateMessage
							{
								IsInitialization = true
							});
						}

						// Do nothing 
						continue;
					}

					adherenceReportSetting = _repositoryFactory.CreateGlobalSettingDataRepository(uow).FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting());
					allAgents = _repositoryFactory.CreatePersonRepository(uow).LoadAll();
				}
				using (var uow = dataSource.Statistic.CreateAndOpenStatelessUnitOfWork())
				{
					var timeZoneList = _repositoryFactory.CreateStatisticRepository().LoadAllTimeZones(uow);
					if (message.IsInitialization)
					{
						foreach (var timezone in timeZoneList)
						{
							var todayForTimezone = DateTime.UtcNow.Date;
							var yesterdayForTimezone = todayForTimezone.AddDays(-1);
							var tomorrowForTimezone = todayForTimezone.AddDays(1).AddMinutes(-timezone.Distance);

							_calculator.Calculate(uow, allAgents, timezone.Id, yesterdayForTimezone, adherenceReportSetting.CalculationMethod);
							_calculator.LastCalculatedDates.Add(timezone.Id, yesterdayForTimezone);
							if (_serviceBus != null)
							{
								_serviceBus.DelaySend(tomorrowForTimezone.ToLocalTime(), new AgentBadgeCalculateMessage
								{
									IsInitialization = false,
									TimezoneId = timezone.Id
								});
							}
						}
					}
					else
					{
						var timezone = timeZoneList.First(tz => tz.Id == message.TimezoneId);
						if (timezone == null) continue;

						var todayForTimezone = DateTime.UtcNow.Date;
						var yesterdayForTimezone = todayForTimezone.AddDays(-1);
						var tomorrowForTimezone = todayForTimezone.AddDays(1).AddMinutes(-timezone.Distance);
						DateTime nextCalculateDate;
						if (!_calculator.LastCalculatedDates.ContainsKey(timezone.Id))
						{
							_calculator.LastCalculatedDates.Add(timezone.Id, tomorrowForTimezone.AddDays(-1).ToLocalTime());
						}
						if (_calculator.LastCalculatedDates[timezone.Id] == tomorrowForTimezone.ToLocalTime())
						{
							nextCalculateDate = tomorrowForTimezone.AddDays(1).ToLocalTime();
						}
						else
						{
							nextCalculateDate = tomorrowForTimezone.ToLocalTime();
							_calculator.LastCalculatedDates[timezone.Id] = nextCalculateDate;
							_calculator.Calculate(uow, allAgents, timezone.Id, yesterdayForTimezone, adherenceReportSetting.CalculationMethod);
						}
						
						if (_serviceBus != null)
						{
							_serviceBus.DelaySend(nextCalculateDate, new AgentBadgeCalculateMessage
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