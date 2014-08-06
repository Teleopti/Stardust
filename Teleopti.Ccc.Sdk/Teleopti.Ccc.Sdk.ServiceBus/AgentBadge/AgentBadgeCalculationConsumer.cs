using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

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
			foreach (var dataSource in GetValidDataSources())
			{
				using (var appuow = dataSource.Application.CreateAndOpenUnitOfWork())
				{
					var calculator = new AgentBadgeCalculator(_repositoryFactory.CreateStatisticRepository());

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

					using (var statisticUow = dataSource.Statistic.CreateAndOpenStatelessUnitOfWork())
					{
						var timeZoneList = _repositoryFactory.CreateStatisticRepository().LoadAllTimeZones(statisticUow);
						if (message.IsInitialization)
						{
							foreach (var timezone in timeZoneList)
							{
								calculateBadgeForTimeZone(statisticUow, appuow, timezone, calculator, agentBadgeSetting, allAgents,
									adherenceReportSetting.CalculationMethod);
							}
						}
						else
						{
							var timezone = timeZoneList.First(tz => tz.Id == message.TimezoneId);
							if (timezone == null) continue;

							calculateBadgeForTimeZone(statisticUow, appuow, timezone, calculator, agentBadgeSetting, allAgents,
								adherenceReportSetting.CalculationMethod);
						}
					}
				}
			}
		}

		private void calculateBadgeForTimeZone(IStatelessUnitOfWork statisticUow, IUnitOfWork appuow, ISimpleTimeZone timezone,
			AgentBadgeCalculator calculator, IAgentBadgeThresholdSettings agentBadgeSetting, IEnumerable<IPerson> allAgents,
			AdherenceReportSettingCalculationMethod adherenceCalculationMethod)
		{
			var todayForTimezone = DateTime.UtcNow.Date;
			var yesterdayForTimezone = todayForTimezone.AddDays(-1);
			var tomorrowForTimezone = todayForTimezone.AddDays(1).AddMinutes(-timezone.Distance);
			DateTime nextCalculateDate;

			if (!calculator.LastCalculatedDates.ContainsKey(timezone.Id))
			{
				calculator.LastCalculatedDates.Add(timezone.Id, tomorrowForTimezone.AddDays(-1).ToLocalTime());
			}

			if (calculator.LastCalculatedDates[timezone.Id] == tomorrowForTimezone.ToLocalTime())
			{
				nextCalculateDate = tomorrowForTimezone.AddDays(1).ToLocalTime();
			}
			else
			{
				nextCalculateDate = tomorrowForTimezone.ToLocalTime();
				calculator.LastCalculatedDates[timezone.Id] = nextCalculateDate;
				var peopleGotABadge = calculator.Calculate(statisticUow, allAgents, timezone.Id, yesterdayForTimezone,
					adherenceCalculationMethod, agentBadgeSetting.SilverBadgeDaysThreshold, agentBadgeSetting.GoldBadgeDaysThreshold);
				foreach (var person in peopleGotABadge)
				{
					SendPushMessageService
						.CreateConversation(Resources.Congratulations, Resources.YouGotNewBadges, false)
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
					TimezoneId = timezone.Id
				});
			}
		}

		protected virtual IEnumerable<IDataSource> GetValidDataSources()
		{
			return StateHolderReader.Instance.StateReader.ApplicationScopeData.RegisteredDataSourceCollection
				.Where(dataSource => dataSource.Statistic != null && dataSource.Application != null);
		}
	}
}