using log4net;
using Rhino.ServiceBus;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Toggle;
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
		private readonly IAgentBadgeWithRankCalculator _badgeWithRankCalculator;
		private readonly INow _now;
		private readonly IAgentBadgeRepository _badgeRepository;
		private readonly IAgentBadgeWithRankRepository _badgeWithRankRepository;
		private readonly IRunningEtlJobChecker _runningEtlJobChecker;
		private readonly IToggleManager _toggleManager;
		private static readonly ILog Logger = LogManager.GetLogger(typeof (CalculateBadgeConsumer));

		public CalculateBadgeConsumer(
			IServiceBus serviceBus,
			IAgentBadgeSettingsRepository settingsRepository,
			IPersonRepository personRepository,
			IGlobalSettingDataRepository globalSettingRep,
			IPushMessagePersister msgPersister,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory,
			IAgentBadgeCalculator calculator,
			IAgentBadgeWithRankCalculator badgeWithRankCalculator,
			IAgentBadgeRepository badgeRepository,
			IAgentBadgeWithRankRepository badgeWithRankRepository,
			IRunningEtlJobChecker runningEtlJobChecker,
			INow now,
			IToggleManager toggleManager)
		{
			_serviceBus = serviceBus;
			_settingsRepository = settingsRepository;
			_personRepository = personRepository;
			_globalSettingRep = globalSettingRep;
			_msgPersister = msgPersister;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_calculator = calculator;
			_badgeWithRankCalculator = badgeWithRankCalculator;
			_now = now;
			_badgeRepository = badgeRepository;
			_badgeWithRankRepository = badgeWithRankRepository;
			_runningEtlJobChecker = runningEtlJobChecker;
			_toggleManager = toggleManager;
		}

		/// <summary>
		/// Calculate the badge stuff
		/// Get the date for doing the next calculation
		/// Delaysend CalculateBadgeMessage to bus for time of next calculation
		/// </summary>
		/// <param name="message"></param>
		public void Consume(CalculateBadgeMessage message)
		{
			var utcNow = _now.UtcDateTime();
			//mutual with feature Portal_DifferentiateBadgeSettingForAgents_31318
			if (_toggleManager.IsEnabled(Toggles.Portal_DifferentiateBadgeSettingForAgents_31318))
				return;

			var toggleCalculateBadgeWithRankEnabled = _toggleManager.IsEnabled(Toggles.Gamification_NewBadgeCalculation_31185);

			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Consume CalculateBadgeMessage with BusinessUnit {0}, DataSource {1} and timezone {2}",
					message.BusinessUnitId, message.Datasource, message.TimeZoneCode);
			}

			// Next badge calculation start at same time point tomorrow.
			var nextMessageShouldBeProcessed = utcNow.AddDays(1);

			var messageForTomorrow = new CalculateBadgeMessage
			{
				Datasource = message.Datasource,
				BusinessUnitId = message.BusinessUnitId,
				Timestamp = utcNow,
				TimeZoneCode = message.TimeZoneCode,
				CalculationDate = message.CalculationDate.AddDays(1)
			};

			using (var uow = _currentUnitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				var setting = _settingsRepository.GetSettings();
				if (setting == null || !setting.BadgeEnabled)
				{
					if (Logger.IsDebugEnabled)
					{
						Logger.DebugFormat("Agent badge is disabled. nothing will be done except send a new CalculateBadgeMessage for "
						                   + "BusinessUnit {0}, DataSource {1} and timezone {2}",
							message.BusinessUnitId, message.Datasource, message.TimeZoneCode);
					}
					resendMessage(messageForTomorrow, nextMessageShouldBeProcessed);
					return;
				}

				var adherenceReportSetting = _globalSettingRep.FindValueByKey(AdherenceReportSetting.Key,
					new AdherenceReportSetting());

				var today = _now.LocalDateOnly();
				var period = new DateOnlyPeriod(today.AddDays(-1), today.AddDays(1));
				var allAgents = _personRepository.FindPeopleInOrganization(period, false);

				var calculateDate = new DateOnly(message.CalculationDate);

				if (_runningEtlJobChecker.NightlyEtlJobStillRunning())
				{
					// If the ETL nightly job still running, then delay badge calculation 5 minutes later.
					if (Logger.IsDebugEnabled)
					{
						Logger.Debug("The \"Nightly\" ETL job is still running, will retry in 5 minutes.");
					}

					var delayedMessage = new CalculateBadgeMessage
					{
						Datasource = message.Datasource,
						BusinessUnitId = message.BusinessUnitId,
						Timestamp = utcNow,
						TimeZoneCode = message.TimeZoneCode,
						CalculationDate = message.CalculationDate
					};
					resendMessage(delayedMessage, utcNow.AddMinutes(5));
					return;
				}

				calculateBadgeForAdherence(message, toggleCalculateBadgeWithRankEnabled, setting, adherenceReportSetting, allAgents,
					calculateDate);

				calculateBadgeForAht(message, toggleCalculateBadgeWithRankEnabled, setting, allAgents, calculateDate);

				calculateBadgeForAnsweredCall(message, toggleCalculateBadgeWithRankEnabled, setting, allAgents, calculateDate);

				uow.PersistAll();
			}

			if (_serviceBus == null) return;

			resendMessage(messageForTomorrow, nextMessageShouldBeProcessed);
		}

		private void calculateBadgeForAnsweredCall(CalculateBadgeMessage message, bool toggleCalculateBadgeWithRankEnabled,
			IAgentBadgeSettings setting, IEnumerable<IPerson> allAgents, DateOnly calculateDate)
		{
			if (!setting.AnsweredCallsBadgeEnabled) return;
			if (toggleCalculateBadgeWithRankEnabled && setting.CalculateBadgeWithRank)
			{
				var newAwardedBadgesWithRankForAnsweredCalls =
					_badgeWithRankCalculator.CalculateAnsweredCallsBadges(allAgents, message.TimeZoneCode, calculateDate,
						setting, message.BusinessUnitId).ToList();
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("Total {0} agents will get new badge for answered calls",
						newAwardedBadgesWithRankForAnsweredCalls.Count());
				}
				sendMessagesToPeopleGotABadge(newAwardedBadgesWithRankForAnsweredCalls, setting, calculateDate,
					BadgeType.AnsweredCalls);
			}
			else
			{
				var newAwardedBadgesForAnsweredCalls = _calculator.CalculateAnsweredCallsBadges(allAgents, message.TimeZoneCode,
					calculateDate, setting, message.BusinessUnitId).ToList();
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("Total {0} agents will get new badge for answered calls",
						newAwardedBadgesForAnsweredCalls.Count());
				}
				sendMessagesToPeopleGotABadge(newAwardedBadgesForAnsweredCalls, setting, calculateDate, BadgeType.AnsweredCalls);
			}
		}

		private void calculateBadgeForAht(CalculateBadgeMessage message, bool toggleCalculateBadgeWithRankEnabled,
			IAgentBadgeSettings setting, IEnumerable<IPerson> allAgents, DateOnly calculateDate)
		{
			if (!setting.AHTBadgeEnabled)
			{
				return;
			}

			if (toggleCalculateBadgeWithRankEnabled && setting.CalculateBadgeWithRank)
			{
				var newAwardedBadgesWithRankForAHT =
					_badgeWithRankCalculator.CalculateAHTBadges(allAgents, message.TimeZoneCode, calculateDate, setting,
						message.BusinessUnitId).ToList();
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("Total {0} agents will get new badge for AHT", newAwardedBadgesWithRankForAHT.Count());
				}
				sendMessagesToPeopleGotABadge(newAwardedBadgesWithRankForAHT, setting, calculateDate, BadgeType.AverageHandlingTime);
			}
			else
			{
				var newAwardedBadgesForAHT =
					_calculator.CalculateAHTBadges(allAgents, message.TimeZoneCode, calculateDate, setting, message.BusinessUnitId)
						.ToList();
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("Total {0} agents will get new badge for AHT", newAwardedBadgesForAHT.Count());
				}
				sendMessagesToPeopleGotABadge(newAwardedBadgesForAHT, setting, calculateDate, BadgeType.AverageHandlingTime);
			}
		}

		private void calculateBadgeForAdherence(CalculateBadgeMessage message, bool toggleCalculateBadgeWithRankEnabled,
			IAgentBadgeSettings setting, AdherenceReportSetting adherenceReportSetting,
			IEnumerable<IPerson> allAgents, DateOnly calculateDate)
		{
			if (!setting.AdherenceBadgeEnabled)
			{
				return;
			}

			if (toggleCalculateBadgeWithRankEnabled && setting.CalculateBadgeWithRank)
			{
				var newAwardedBadgesWithRankForAdherence =
					_badgeWithRankCalculator.CalculateAdherenceBadges(allAgents, message.TimeZoneCode, calculateDate,
						adherenceReportSetting.CalculationMethod, setting, message.BusinessUnitId).ToList();
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("Total {0} agents will get new badge for adherence",
						newAwardedBadgesWithRankForAdherence.Count());
				}
				sendMessagesToPeopleGotABadge(newAwardedBadgesWithRankForAdherence, setting, calculateDate, BadgeType.Adherence);
			}
			else
			{
				var newAwardedBadgesForAdherence =
					_calculator.CalculateAdherenceBadges(allAgents, message.TimeZoneCode, calculateDate,
						adherenceReportSetting.CalculationMethod, setting, message.BusinessUnitId).ToList();
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("Total {0} agents will get new badge for adherence", newAwardedBadgesForAdherence.Count());
				}
				sendMessagesToPeopleGotABadge(newAwardedBadgesForAdherence, setting, calculateDate, BadgeType.Adherence);
			}
		}

		private void resendMessage(CalculateBadgeMessage message, DateTime delaySendTime)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat(
					"Delay Sending CalculateBadgeMessage to Service Bus for Timezone={0} "
					+ "on next calculation time={1:yyyy-MM-dd HH:mm:ss}",
					message.TimeZoneCode, delaySendTime);
			}

			_serviceBus.DelaySend(delaySendTime, message);
		}

		private void sendMessagesToPeopleGotABadge(IEnumerable<IAgentBadgeTransaction> newAwardedBadges,
			IAgentBadgeSettings setting, DateOnly calculateDate, BadgeType badgeType)
		{
			var agentBadgeTransactions = newAwardedBadges as IList<IAgentBadgeTransaction> ?? newAwardedBadges.ToList();

			var existedBadges = _badgeRepository.Find(agentBadgeTransactions.Select(x => x.Person.Id.GetValueOrDefault()),
				badgeType);
			foreach (var badgeTransaction in agentBadgeTransactions)
			{
				var person = badgeTransaction.Person;

				var existedBadge = existedBadges.SingleOrDefault(x => x.Person == person.Id) ?? new Domain.Common.AgentBadge
				{
					Person = person.Id.GetValueOrDefault(),
					TotalAmount = 0,
					BadgeType = badgeType
				};

				existedBadge.TotalAmount += badgeTransaction.Amount;

				var bronzeBadgeMessageTemplate = string.Empty;
				var silverBadgeMessageTemplate = string.Empty;
				var goldBadgeMessageTemplate = string.Empty;
				var threshold = string.Empty;

				BadgeRank badgeRank;
				string message;

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
					badgeRank = BadgeRank.Bronze;
					message = string.Format(bronzeBadgeMessageTemplate, threshold, calculateDate.Date);

					sendBadgeMessage(person, badgeType, badgeRank, message);
				}

				if (existedBadge.IsSilverBadgeAdded(setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate))
				{
					badgeRank = BadgeRank.Silver;
					message = string.Format(silverBadgeMessageTemplate, threshold, setting.SilverToBronzeBadgeRate);

					sendBadgeMessage(person, badgeType, badgeRank, message);
				}

				if (existedBadge.IsGoldBadgeAdded(setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate))
				{
					badgeRank = BadgeRank.Gold;
					message = string.Format(goldBadgeMessageTemplate, threshold,
						setting.SilverToBronzeBadgeRate*setting.GoldToSilverBadgeRate);

					sendBadgeMessage(person, badgeType, badgeRank, message);
				}
			}
		}

		private void sendMessagesToPeopleGotABadge(IEnumerable<IAgentBadgeWithRankTransaction> newAwardedBadges,
			IAgentBadgeSettings setting, DateOnly calculateDate, BadgeType badgeType)
		{
			var agentBadgeWithRankTransactions = newAwardedBadges as IList<IAgentBadgeWithRankTransaction>
			                                     ?? newAwardedBadges.ToList();

			var existedBadges = _badgeWithRankRepository.Find(agentBadgeWithRankTransactions
				.Select(x => x.Person.Id.GetValueOrDefault()), badgeType);
			foreach (var badgeTransaction in agentBadgeWithRankTransactions)
			{
				var person = badgeTransaction.Person;

				var existedBadge = existedBadges.SingleOrDefault(x => x.Person == person.Id) ?? new Domain.Common.AgentBadgeWithRank
				{
					Person = person.Id.GetValueOrDefault(),
					BronzeBadgeAmount = 0,
					SilverBadgeAmount = 0,
					GoldBadgeAmount = 0,
					BadgeType = badgeType
				};

				existedBadge.BronzeBadgeAmount += badgeTransaction.BronzeBadgeAmount;
				existedBadge.SilverBadgeAmount += badgeTransaction.SilverBadgeAmount;
				existedBadge.GoldBadgeAmount += badgeTransaction.GoldBadgeAmount;

				var bronzeBadgeMessageTemplate = string.Empty;
				var silverBadgeMessageTemplate = string.Empty;
				var goldBadgeMessageTemplate = string.Empty;

				var threshold = string.Empty;
				var bronzeBadgeThreshold = string.Empty;
				var silverBadgeThreshold = string.Empty;
				var goldBadgeThreshold = string.Empty;

				var badgeRank = BadgeRank.Bronze;
				var messageTemplate = string.Empty;

				switch (badgeType)
				{
					case BadgeType.AverageHandlingTime:
						bronzeBadgeMessageTemplate = Resources.BadgeWithRank_YouGotANewBronzeBadgeForAHT;
						silverBadgeMessageTemplate = Resources.BadgeWithRank_YouGotANewSilverBadgeForAHT;
						goldBadgeMessageTemplate = Resources.BadgeWithRank_YouGotANewGoldBadgeForAHT;

						bronzeBadgeThreshold = setting.AHTBronzeThreshold.TotalSeconds.ToString(CultureInfo.InvariantCulture);
						silverBadgeThreshold = setting.AHTSilverThreshold.TotalSeconds.ToString(CultureInfo.InvariantCulture);
						goldBadgeThreshold = setting.AHTGoldThreshold.TotalSeconds.ToString(CultureInfo.InvariantCulture);
						break;
					case BadgeType.AnsweredCalls:
						bronzeBadgeMessageTemplate = Resources.BadgeWithRank_YouGotANewBronzeBadgeForAnsweredCalls;
						silverBadgeMessageTemplate = Resources.BadgeWithRank_YouGotANewSilverBadgeForAnsweredCalls;
						goldBadgeMessageTemplate = Resources.BadgeWithRank_YouGotANewGoldBadgeForAnsweredCalls;

						bronzeBadgeThreshold = setting.AnsweredCallsBronzeThreshold.ToString(CultureInfo.InvariantCulture);
						silverBadgeThreshold = setting.AnsweredCallsSilverThreshold.ToString(CultureInfo.InvariantCulture);
						goldBadgeThreshold = setting.AnsweredCallsGoldThreshold.ToString(CultureInfo.InvariantCulture);
						break;
					case BadgeType.Adherence:
						bronzeBadgeMessageTemplate = Resources.BadgeWithRank_YouGotANewBronzeBadgeForAdherence;
						silverBadgeMessageTemplate = Resources.BadgeWithRank_YouGotANewSilverBadgeForAdherence;
						goldBadgeMessageTemplate = Resources.BadgeWithRank_YouGotANewGoldBadgeForAdherence;

						bronzeBadgeThreshold = setting.AdherenceBronzeThreshold.ToString();
						silverBadgeThreshold = setting.AdherenceSilverThreshold.ToString();
						goldBadgeThreshold = setting.AdherenceGoldThreshold.ToString();
						break;
				}

				if (existedBadge.IsBronzeBadgeAdded)
				{
					badgeRank = BadgeRank.Bronze;
					threshold = bronzeBadgeThreshold;
					messageTemplate = bronzeBadgeMessageTemplate;
				}
				else if (existedBadge.IsSilverBadgeAdded)
				{
					badgeRank = BadgeRank.Silver;
					threshold = silverBadgeThreshold;
					messageTemplate = silverBadgeMessageTemplate;
				}
				else if (existedBadge.IsGoldBadgeAdded)
				{
					badgeRank = BadgeRank.Gold;
					threshold = goldBadgeThreshold;
					messageTemplate = goldBadgeMessageTemplate;
				}

				var message = string.Format(messageTemplate, threshold, calculateDate.Date);
				sendBadgeMessage(person, badgeType, badgeRank, message);
			}
		}

		private MessageType getMessageType(BadgeType badgeType, BadgeRank badgeRank)
		{
			var messageType = MessageType.Information;
			switch (badgeRank)
			{
				case BadgeRank.Bronze:
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
					break;
				case BadgeRank.Silver:
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
					break;
				case BadgeRank.Gold:
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
					break;
			}

			return messageType;
		}

		private void sendBadgeMessage(IPerson person, BadgeType badgeType, BadgeRank badgeRank, string message)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Send {3} badge message to agent {0} (ID: {1}) for badge type: {2}", person.Name, person.Id,
					badgeType, badgeRank);
			}

			var messageType = getMessageType(badgeType, badgeRank);

			SendPushMessageService
				.CreateConversation(Resources.Congratulations, message, false, messageType)
				.To(person)
				.SendConversation(_msgPersister);
		}

		private enum BadgeRank
		{
			Bronze = 0,
			Silver = 1,
			Gold = 2
		}
	}
}
