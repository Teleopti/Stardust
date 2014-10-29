﻿using log4net;
using Rhino.ServiceBus;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class CalculateBadgeConsumer : ConsumerOf<CalculateBadgeMessage>
	{
		private readonly IServiceBus _serviceBus;
		private readonly IAgentBadgeSettingsRepository _settingsRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IGlobalSettingDataRepository _globalSettingRep;
		private readonly IPushMessagePersister _msgPersister;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IAgentBadgeCalculator _calculator;
		private readonly INow _now;
		private readonly IAgentBadgeRepository _badgeRepository;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(CalculateBadgeConsumer));

		public CalculateBadgeConsumer(
									IServiceBus serviceBus, 
									IAgentBadgeSettingsRepository settingsRepository,
									IPersonRepository personRepository, 
									IGlobalSettingDataRepository globalSettingRep,
									IPushMessagePersister msgPersister, 
									ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
									IAgentBadgeCalculator calculator,
									IAgentBadgeRepository badgeRepository,
									INow now)
		{
			_serviceBus = serviceBus;
			_settingsRepository = settingsRepository;
			_personRepository = personRepository;
			_globalSettingRep = globalSettingRep;
			_msgPersister = msgPersister;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_calculator = calculator;
			_now = now;
			_badgeRepository = badgeRepository;
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

			var setting = _settingsRepository.GetSettings();
			if (setting == null)
			{
				//error happens
				Logger.Error("Agent badge threshold setting is null before starting badge calculation");
				return;
			}

			if (!setting.BadgeEnabled)
			{
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("Agent badge is disabled. nothing will be done except send a new CalculateBadgeMessage for "
					                   + "BusinessUnit {0}, DataSource {1} and timezone {2}",
						message.BusinessUnitId, message.Datasource, message.TimeZoneCode);
				}
				resendMessage(message);
				return;
			}

			using (var uow = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var adherenceReportSetting = _globalSettingRep.FindValueByKey(AdherenceReportSetting.Key,
					new AdherenceReportSetting());

				var today = _now.LocalDateOnly();
				var allAgents = _personRepository.FindPeopleInOrganization(new DateOnlyPeriod(today.AddDays(-1), today.AddDays(1)), false);

				var calculateDate = new DateOnly(message.CalculationDate);

				if (setting.AdherenceBadgeEnabled)
				{
					var newAwardedBadgesForAdherence =
						_calculator.CalculateAdherenceBadges(allAgents, message.TimeZoneCode, calculateDate,
							adherenceReportSetting.CalculationMethod, setting).ToList();
					if (Logger.IsDebugEnabled)
					{
						Logger.DebugFormat("Total {0} agents will get new badge for adherence", newAwardedBadgesForAdherence.Count());
					}
					sendMessagesToPeopleGotABadge(newAwardedBadgesForAdherence, setting, calculateDate, BadgeType.Adherence);
				}

				if (setting.AHTBadgeEnabled)
				{
					var newAwardedBadgesForAHT =
						_calculator.CalculateAHTBadges(allAgents, message.TimeZoneCode, calculateDate, setting).ToList();
					if (Logger.IsDebugEnabled)
					{
						Logger.DebugFormat("Total {0} agents will get new badge for AHT", newAwardedBadgesForAHT.Count());
					}
					sendMessagesToPeopleGotABadge(newAwardedBadgesForAHT, setting, calculateDate, BadgeType.AverageHandlingTime);
				}

				if (setting.AnsweredCallsBadgeEnabled)
				{
					var newAwardedBadgesForAnsweredCalls = _calculator.CalculateAnsweredCallsBadges(allAgents, message.TimeZoneCode,
						calculateDate, setting).ToList();
					if (Logger.IsDebugEnabled)
					{
						Logger.DebugFormat("Total {0} agents will get new badge for answered calls",
							newAwardedBadgesForAnsweredCalls.Count());
					}
					sendMessagesToPeopleGotABadge(newAwardedBadgesForAnsweredCalls, setting, calculateDate, BadgeType.AnsweredCalls);
				}

				uow.PersistAll();
			}

			if (_serviceBus == null) return;

			resendMessage(message);
		}

		private void resendMessage(CalculateBadgeMessage message)
		{
			var today = _now.LocalDateOnly();
			var tomorrow = new DateTime(today.AddDays(1).Date.Ticks, DateTimeKind.Unspecified);
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(message.TimeZoneCode);

			// Set badge calculation start at 5:00 AM
			// Just hard code it now, the best solution is to trigger it from ETL
			var nextMessageShouldBeProcessed = TimeZoneInfo.ConvertTime(tomorrow.AddHours(5), timeZone, TimeZoneInfo.Local);
			var newMessage = new CalculateBadgeMessage
			{
				Datasource = message.Datasource,
				BusinessUnitId = message.BusinessUnitId,
				Timestamp = DateTime.UtcNow,
				TimeZoneCode = message.TimeZoneCode,
				CalculationDate = message.CalculationDate.AddDays(1)
			};

			_serviceBus.DelaySend(nextMessageShouldBeProcessed, newMessage);

			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat(
						"Delay Sending CalculateBadgeMessage to Service Bus for Timezone={0} on next calculation time={1:yyyy-MM-dd HH:mm:ss}",
						newMessage.TimeZoneCode, nextMessageShouldBeProcessed);
			}
		}

		private void sendMessagesToPeopleGotABadge(IEnumerable<IAgentBadgeTransaction> newAwardedBadges,
			IAgentBadgeSettings setting, DateOnly calculateDate, BadgeType badgeType)
		{
			var agentBadgeTransactions = newAwardedBadges as IList<IAgentBadgeTransaction> ?? newAwardedBadges.ToList();

			var existedBadges = _badgeRepository.Find(agentBadgeTransactions.Select(
				x => x.Person.Id != null ? (Guid)x.Person.Id : new Guid()), badgeType);
			foreach (var badgeTransaction in agentBadgeTransactions)
			{
				var person = badgeTransaction.Person;

				var existedBadge = existedBadges.SingleOrDefault(x => x.Person == person.Id) ?? new Domain.Common.AgentBadge
				{
					Person = (Guid)person.Id,
					TotalAmount = 0,
					BadgeType = badgeType
				};

				existedBadge.TotalAmount += badgeTransaction.Amount;

				var bronzeBadgeMessageTemplate = string.Empty;
				var silverBadgeMessageTemplate = string.Empty;
				var goldBadgeMessageTemplate = string.Empty;
				var threshold = string.Empty;

				switch (badgeType)
				{
					case BadgeType.AverageHandlingTime:
						bronzeBadgeMessageTemplate = Resources.YouGotANewBronzeBadgeForAHT;
						silverBadgeMessageTemplate = Resources.YouGotANewSilverBadgeForAHT;
						goldBadgeMessageTemplate = Resources.YouGotANewGoldBadgeForAHT;
						threshold = setting.AHTThreshold.TotalSeconds.ToString(CultureInfo.InvariantCulture);
						break;
					case BadgeType.AnsweredCalls:
						bronzeBadgeMessageTemplate = Resources.YouGotANewBronzeBadgeForAnsweredCalls;
						silverBadgeMessageTemplate = Resources.YouGotANewSilverBadgeForAnsweredCalls;
						goldBadgeMessageTemplate = Resources.YouGotANewGoldBadgeForAnsweredCalls;
						threshold = setting.AnsweredCallsThreshold.ToString(CultureInfo.InvariantCulture);
						break;
					case BadgeType.Adherence:
						bronzeBadgeMessageTemplate = Resources.YouGotANewBronzeBadgeForAdherence;
						silverBadgeMessageTemplate = Resources.YouGotANewSilverBadgeForAdherence;
						goldBadgeMessageTemplate = Resources.YouGotANewGoldBadgeForAdherence;
						threshold = setting.AdherenceThreshold.ToString();
						break;
				}

				if (existedBadge.IsBronzeBadgeAdded(setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate))
				{
					var message = string.Format(bronzeBadgeMessageTemplate, threshold, calculateDate.Date);
					sendBronzeBadgeMessage(person, badgeType, message);
				}

				if (existedBadge.IsSilverBadgeAdded(setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate))
				{
					var message = string.Format(silverBadgeMessageTemplate, threshold, setting.SilverToBronzeBadgeRate);
					sendSilverBadgeMessage(person, badgeType, message);
				}

				if (existedBadge.IsGoldBadgeAdded(setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate))
				{
					var message = string.Format(goldBadgeMessageTemplate, threshold,
						setting.SilverToBronzeBadgeRate * setting.GoldToSilverBadgeRate);
					sendGoldBadgeMessage(person, badgeType, message);
				}
			}
		}

		private void sendBronzeBadgeMessage(IPerson person, BadgeType badgeType, string message)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Send bronze badge message to agent {0} (ID: {1}) for badge type: {2}", person.Name, person.Id,
					badgeType);
			}

			var messageType = MessageType.Information;
			switch (badgeType)
			{
				case BadgeType.Adherence:
					messageType = MessageType.AdherenceBronzeBadge;
					break;
				case BadgeType.AverageHandlingTime:
					messageType = MessageType.AHTBronzeBadge;
					break;
				case BadgeType.AnsweredCalls:
					messageType = MessageType.AnsweredCallsBronzeBadge;
					break;
			}

			SendPushMessageService
				.CreateConversation(Resources.Congratulations, message, false, messageType)
				.To(person)
				.SendConversation(_msgPersister);
		}

		private void sendSilverBadgeMessage(IPerson person, BadgeType badgeType, string message)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Send silver badge message to agent {0} (ID: {1}) for badge type: {2}", person.Name, person.Id,
					badgeType);
			}

			var messageType = MessageType.Information;
			switch (badgeType)
			{
				case BadgeType.Adherence:
					messageType = MessageType.AdherenceSilverBadge;
					break;
				case BadgeType.AverageHandlingTime:
					messageType = MessageType.AHTSilverBadge;
					break;
				case BadgeType.AnsweredCalls:
					messageType = MessageType.AnsweredCallsSilverBadge;
					break;
			}

			SendPushMessageService
				.CreateConversation(Resources.Congratulations, message, false, messageType)
				.To(person)
				.SendConversation(_msgPersister);
		}

		private void sendGoldBadgeMessage(IPerson person, BadgeType badgeType, string message)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Send gold badge message to agent {0} (ID: {1}) for badge type: {2}", person.Name, person.Id,
					badgeType);
			}

			var messageType = MessageType.Information;
			switch (badgeType)
			{
				case BadgeType.Adherence:
					messageType = MessageType.AdherenceGoldBadge;
					break;
				case BadgeType.AverageHandlingTime:
					messageType = MessageType.AHTGoldBadge;
					break;
				case BadgeType.AnsweredCalls:
					messageType = MessageType.AnsweredCallsGoldBadge;
					break;
			}

			SendPushMessageService
				.CreateConversation(Resources.Congratulations, message, false, messageType)
				.To(person)
				.SendConversation(_msgPersister);
		}
	}
}
