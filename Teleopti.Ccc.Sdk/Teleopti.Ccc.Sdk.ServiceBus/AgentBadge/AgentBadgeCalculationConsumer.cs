﻿using System;
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
							var todayForTimezone = DateTime.UtcNow.Date;
							var yesterdayForTimezone = todayForTimezone.AddDays(-1);
							var tomorrowForTimezone = todayForTimezone.AddDays(1).AddMinutes(-timezone.Item3);

							_calculator.Calculate(uow, allAgents, timezone.Item1, yesterdayForTimezone, adherenceReportSetting.CalculationMethod);
							_calculator.LastCalculatedDates.Add(timezone.Item1, yesterdayForTimezone);
							if (_serviceBus != null)
							{
								_serviceBus.DelaySend(tomorrowForTimezone.ToLocalTime(), new AgentBadgeCalculateMessage
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

						var todayForTimezone = DateTime.UtcNow.Date;
						var yesterdayForTimezone = todayForTimezone.AddDays(-1);
						var tomorrowForTimezone = todayForTimezone.AddDays(1).AddMinutes(-timezone.Item3);
						DateTime nextCalculateDate;
						if (!_calculator.LastCalculatedDates.ContainsKey(timezone.Item1))
						{
							_calculator.LastCalculatedDates.Add(timezone.Item1, tomorrowForTimezone.AddDays(-1).ToLocalTime());
						}
						if (_calculator.LastCalculatedDates[timezone.Item1] == tomorrowForTimezone.ToLocalTime())
						{
							nextCalculateDate = tomorrowForTimezone.AddDays(1).ToLocalTime();
						}
						else
						{
							nextCalculateDate = tomorrowForTimezone.ToLocalTime();
							_calculator.LastCalculatedDates[timezone.Item1] = nextCalculateDate;
							_calculator.Calculate(uow, allAgents, timezone.Item1, yesterdayForTimezone, adherenceReportSetting.CalculationMethod);
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