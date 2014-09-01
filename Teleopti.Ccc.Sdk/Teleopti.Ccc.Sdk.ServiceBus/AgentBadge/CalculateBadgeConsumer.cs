using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class CalculateBadgeConsumer : ConsumerOf<CalculateBadgeMessage>
	{
		private readonly IServiceBus _serviceBus;
		private readonly IAgentBadgeSettingsRepository _settingsRepository;
		private readonly IAgentBadgeRepository _badgeRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IGlobalSettingDataRepository _globalSettingRep;
		private readonly IPushMessagePersister _msgPersister;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IAgentBadgeCalculator _calculator;
		private readonly INow _now;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(CalculateBadgeConsumer));

		public CalculateBadgeConsumer(
									IServiceBus serviceBus, 
									IAgentBadgeSettingsRepository settingsRepository, 
									IAgentBadgeRepository badgeRepository,
									IPersonRepository personRepository, 
									IGlobalSettingDataRepository globalSettingRep,
									IPushMessagePersister msgPersister, 
									ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
									IAgentBadgeCalculator calculator,
									INow now)
		{
			_serviceBus = serviceBus;
			_settingsRepository = settingsRepository;
			_badgeRepository = badgeRepository;
			_personRepository = personRepository;
			_globalSettingRep = globalSettingRep;
			_msgPersister = msgPersister;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_calculator = calculator;
			_now = now;
		}

		/// <summary>
		/// Calculate the badge stuff
		/// Get the date for doing the next calculation
		/// Delaysend CalculateBadgeMessage to bus for time of next calculation
		/// </summary>
		/// <param name="message"></param>
		public void Consume(CalculateBadgeMessage message)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Consume CalculateBadgeMessage with BusinessUnit {0}, DataSource {1} and timezone {2}", message.BusinessUnitId,
					message.Datasource, message.TimeZoneCode);
			}

			var today = _now.LocalDateOnly();
			var tomorrow = new DateTime(today.AddDays(1).Date.Ticks, DateTimeKind.Unspecified);
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(message.TimeZoneCode);

			// Set badge calculation start at 5:00 AM
			// Just hard code it now, the best solution is to trigger it from ETL
			var nextMessageShouldBeProcessed =
				TimeZoneInfo.ConvertTime(tomorrow.AddHours(5), timeZone, TimeZoneInfo.Local);

			using (var uow = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var setting = _settingsRepository.LoadAll().FirstOrDefault();
				if (setting == null)
				{
					//error happens
					Logger.Error("Agent badge threshold setting is null before starting badge calculation");
					return;
				}
				var adherenceReportSetting = _globalSettingRep.FindValueByKey(AdherenceReportSetting.Key, new AdherenceReportSetting());

				ICollection<IPerson> allAgents = _personRepository.FindPeopleInOrganization(new DateOnlyPeriod(today.AddDays(-1), today.AddDays(1)), false);

				var calculateDate = new DateOnly(message.CalculationDate);
				var newAwardedBadgesForAdherence = _calculator.CalculateAdherenceBadges(allAgents, message.TimeZoneCode, calculateDate,
					adherenceReportSetting.CalculationMethod, setting).ToList();
				var newAwardedBadgesForAHT = _calculator.CalculateAHTBadges(allAgents, message.TimeZoneCode, calculateDate, setting).ToList();
				var newAwardedBadgesForAnsweredCalls = _calculator.CalculateAnsweredCallsBadges(allAgents, message.TimeZoneCode,
					calculateDate, setting).ToList();

				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("Total {0} agents will get new badge for adherence", newAwardedBadgesForAdherence.Count());
					Logger.DebugFormat("Total {0} agents will get new badge for AHT", newAwardedBadgesForAHT.Count());
					Logger.DebugFormat("Total {0} agents will get new badge for answered calls", newAwardedBadgesForAnsweredCalls.Count());
				}

				// send message
				sendMessagesToPeopleGotABadge(newAwardedBadgesForAHT, setting);
				sendMessagesToPeopleGotABadge(newAwardedBadgesForAdherence, setting);
				sendMessagesToPeopleGotABadge(newAwardedBadgesForAnsweredCalls, setting);

				uow.PersistAll();
			}

			if (_serviceBus == null) return;

			_serviceBus.DelaySend(nextMessageShouldBeProcessed, new CalculateBadgeMessage
			{
				Datasource = message.Datasource,
				BusinessUnitId = message.BusinessUnitId,
				Timestamp = DateTime.UtcNow,
				TimeZoneCode = message.TimeZoneCode,
				CalculationDate = message.CalculationDate.AddDays(1)
			});

			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat(
						"Delay Sending CalculateBadgeMessage to Service Bus for Timezone={0} on next calculation time={1:yyyy-MM-dd HH:mm:ss}", message.TimeZoneCode,
						nextMessageShouldBeProcessed);
			}
		}

		private void sendMessagesToPeopleGotABadge(IEnumerable<IAgentBadge> newAwardedBadges, IAgentBadgeThresholdSettings setting)
		{
			foreach (var badge in newAwardedBadges)
			{
				var person = badge.Person;
				var badgeType = badge.BadgeType;
				switch (badgeType)
				{
					case BadgeType.AverageHandlingTime:
						if (badge.BronzeBadgeAdded)
						{
							if (Logger.IsDebugEnabled)
							{
								Logger.DebugFormat("Send bronze badge message to agent {0} (ID: {1}) for badge type: {2}", person.Name, person.Id, badgeType);
							}
							SendPushMessageService
								.CreateConversation(Resources.Congratulations,
									string.Format(Resources.YouGotANewBronzeBadgeForAHT, setting.AHTThreshold.TotalSeconds), false,
									MessageType.AHTBronzeBadge)
								.To(person)
								.SendConversation(_msgPersister);
						}
						if (badge.SilverBadgeAdded)
						{
							if (Logger.IsDebugEnabled)
							{
								Logger.DebugFormat("Send silver badge message to agent {0} (ID: {1}) for badge type: {2}", person.Name, person.Id, badgeType);
							}
							SendPushMessageService
								.CreateConversation(Resources.Congratulations,
									string.Format(Resources.YouGotANewSilverBadgeForAHT, setting.AHTThreshold.TotalSeconds,
										setting.SilverToBronzeBadgeRate), false, MessageType.AHTSilverBadge)
								.To(person)
								.SendConversation(_msgPersister);
						}
						if (badge.GoldBadgeAdded)
						{
							if (Logger.IsDebugEnabled)
							{
								Logger.DebugFormat("Send gold badge message to agent {0} (ID: {1}) for badge type: {2}", person.Name, person.Id, badgeType);
							}
							SendPushMessageService
								.CreateConversation(Resources.Congratulations,
									string.Format(Resources.YouGotANewGoldBadgeForAHT, setting.AHTThreshold.TotalSeconds,
										setting.SilverToBronzeBadgeRate*setting.GoldToSilverBadgeRate), false, MessageType.AHTGoldBadge)
								.To(person)
								.SendConversation(_msgPersister);
						}
						break;
					case BadgeType.AnsweredCalls:
						if (badge.BronzeBadgeAdded)
						{
							if (Logger.IsDebugEnabled)
							{
								Logger.DebugFormat("Send bronze badge message to agent {0} (ID: {1}) for badge type: {2}", person.Name, person.Id, badgeType);
							}
							SendPushMessageService
								.CreateConversation(Resources.Congratulations,
									string.Format(Resources.YouGotANewBronzeBadgeForAnsweredCalls, setting.AnsweredCallsThreshold), false,
									MessageType.AnsweredCallsBronzeBadge)
								.To(person)
								.SendConversation(_msgPersister);
						}
						if (badge.SilverBadgeAdded)
						{
							if (Logger.IsDebugEnabled)
							{
								Logger.DebugFormat("Send silver badge message to agent {0} (ID: {1}) for badge type: {2}", person.Name, person.Id, badgeType);
							}
							SendPushMessageService
								.CreateConversation(Resources.Congratulations,
									string.Format(Resources.YouGotANewSilverBadgeForAnsweredCalls, setting.AnsweredCallsThreshold,
										setting.SilverToBronzeBadgeRate), false, MessageType.AnsweredCallsSilverBadge)
								.To(person)
								.SendConversation(_msgPersister);
						}
						if (badge.GoldBadgeAdded)
						{
							if (Logger.IsDebugEnabled)
							{
								Logger.DebugFormat("Send gold badge message to agent {0} (ID: {1}) for badge type: {2}", person.Name, person.Id, badgeType);
							}
							SendPushMessageService
								.CreateConversation(Resources.Congratulations,
									string.Format(Resources.YouGotANewGoldBadgeForAnsweredCalls, setting.AnsweredCallsThreshold,
										setting.SilverToBronzeBadgeRate*setting.GoldToSilverBadgeRate), false, MessageType.AnsweredCallsGoldBadge)
								.To(person)
								.SendConversation(_msgPersister);
						}
						break;
					case BadgeType.Adherence:
						if (badge.BronzeBadgeAdded)
						{
							if (Logger.IsDebugEnabled)
							{
								Logger.DebugFormat("Send bronze badge message to agent {0} (ID: {1}) for badge type: {2}", person.Name, person.Id, badgeType);
							}
							SendPushMessageService
								.CreateConversation(Resources.Congratulations,
									string.Format(Resources.YouGotANewBronzeBadgeForAdherence, setting.AdherenceThreshold.ValueAsPercent()), false,
									MessageType.AdherenceBronzeBadge)
								.To(person)
								.SendConversation(_msgPersister);
						}
						if (badge.SilverBadgeAdded)
						{
							if (Logger.IsDebugEnabled)
							{
								Logger.DebugFormat("Send silver badge message to agent {0} (ID: {1}) for badge type: {2}", person.Name, person.Id, badgeType);
							}
							SendPushMessageService
								.CreateConversation(Resources.Congratulations,
									string.Format(Resources.YouGotANewSilverBadgeForAdherence, setting.AdherenceThreshold.ValueAsPercent(),
										setting.SilverToBronzeBadgeRate), false, MessageType.AdherenceSilverBadge)
								.To(person)
								.SendConversation(_msgPersister);
						}
						if (badge.GoldBadgeAdded)
						{
							if (Logger.IsDebugEnabled)
							{
								Logger.DebugFormat("Send gold badge message to agent {0} (ID: {1}) for badge type: {2}", person.Name, person.Id, badgeType);
							}
							SendPushMessageService
								.CreateConversation(Resources.Congratulations,
									string.Format(Resources.YouGotANewGoldBadgeForAdherence, setting.AdherenceThreshold.ValueAsPercent(),
										setting.SilverToBronzeBadgeRate*setting.GoldToSilverBadgeRate), false, MessageType.AdherenceGoldBadge)
								.To(person)
								.SendConversation(_msgPersister);
						}
						break;
				}
			}
		}
	}
}