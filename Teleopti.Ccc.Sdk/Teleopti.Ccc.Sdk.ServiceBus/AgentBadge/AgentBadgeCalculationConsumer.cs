﻿using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.UserTexts;
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
			foreach (
				var dataSource in
					GetRegisteredDataSourceCollection()
						.Where(dataSource => dataSource.Statistic != null && dataSource.Application != null))
			{
				using (var appuow = dataSource.Application.CreateAndOpenUnitOfWork())
				{
					var agentBadgeSetting = _repositoryFactory.CreateAgentBadgeSettingsRepository(appuow).LoadAll().FirstOrDefault();
					if (agentBadgeSetting == null || !agentBadgeSetting.EnableBadge)
					{
						// If no setting for agent badge or agent badge disabled
						// Then send message for next day to enable badge calculation (the badge feature may be enabled in this period).
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

					var adherenceReportSetting = _repositoryFactory.CreateGlobalSettingDataRepository(appuow)
						.FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting());
					var allAgents = _repositoryFactory.CreatePersonRepository(appuow).LoadAll();

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

								var peopleGotABadge = _calculator.Calculate(uow, allAgents, timezone.Id, yesterdayForTimezone, adherenceReportSetting.CalculationMethod);
								_calculator.LastCalculatedDates.Add(timezone.Id, yesterdayForTimezone);
								foreach (var person in peopleGotABadge)
								{
									SendPushMessageService.CreateConversation(Resources.Congratulations, Resources.YouGotNewBadges, false)
									.To(person)
									.SendConversation(_repositoryFactory.CreatePushMessageRepository(appuow));
									appuow.PersistAll();
								}
								if (_serviceBus != null)
								{
									var nextCalculateDate = tomorrowForTimezone.ToLocalTime().Add(agentBadgeSetting.CalculationTime);
									_serviceBus.DelaySend(nextCalculateDate, new AgentBadgeCalculateMessage
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
								var peopleGotABadge = _calculator.Calculate(uow, allAgents, timezone.Id, yesterdayForTimezone, adherenceReportSetting.CalculationMethod);
								foreach (var person in peopleGotABadge)
								{
									SendPushMessageService.CreateConversation(Resources.Congratulations, Resources.YouGotNewBadges, false)
									.To(person)
									.SendConversation(_repositoryFactory.CreatePushMessageRepository(appuow));
									appuow.PersistAll();
								}
							}

							if (_serviceBus != null)
							{
								nextCalculateDate = nextCalculateDate.Add(agentBadgeSetting.CalculationTime);
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
		}

		protected virtual IEnumerable<IDataSource> GetRegisteredDataSourceCollection()
		{
			return StateHolderReader.Instance.StateReader.ApplicationScopeData.RegisteredDataSourceCollection;
		}
	}
}