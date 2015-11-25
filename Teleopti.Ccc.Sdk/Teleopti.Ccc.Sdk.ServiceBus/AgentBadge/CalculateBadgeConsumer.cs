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
		private readonly ITeamGamificationSettingRepository _teamSettingsRepository;
		private readonly IPushMessagePersister _msgPersister;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IAgentBadgeCalculator _calculator;
		private readonly IAgentBadgeWithRankCalculator _badgeWithRankCalculator;
		private readonly IAgentBadgeRepository _badgeRepository;
		private readonly IAgentBadgeWithRankRepository _badgeWithRankRepository;
		private readonly INow _now;
		private readonly IRunningEtlJobChecker _runningEtlJobChecker;
		private readonly IToggleManager _toggleManager;
		private static readonly ILog logger = LogManager.GetLogger(typeof(CalculateBadgeConsumer));
		private readonly IGlobalSettingDataRepository _globalSettingRep;
		private readonly IPersonRepository _personRepository;

		public CalculateBadgeConsumer(IServiceBus serviceBus,
			ITeamGamificationSettingRepository teamSettingsRepository,
			IPushMessagePersister msgRepository,
			ICurrentUnitOfWorkFactory unitOfWorkFactory,
			IAgentBadgeCalculator calculator,
			IAgentBadgeWithRankCalculator badgeWithRankCalculator,
			IAgentBadgeRepository badgeRepository,
			IAgentBadgeWithRankRepository badgeWithRankRepository,
			INow now,
			IRunningEtlJobChecker runningEtlJobChecker,
			IToggleManager toggleManager,
			IGlobalSettingDataRepository globalSettingRep,
			IPersonRepository personRepository
			)
		{
			_serviceBus = serviceBus;
			_teamSettingsRepository = teamSettingsRepository;
			_msgPersister = msgRepository;
			_unitOfWorkFactory = unitOfWorkFactory;
			_calculator = calculator;
			_badgeWithRankCalculator = badgeWithRankCalculator;
			_badgeRepository = badgeRepository;
			_badgeWithRankRepository = badgeWithRankRepository;
			_now = now;
			_runningEtlJobChecker = runningEtlJobChecker;
			_toggleManager = toggleManager;
			_globalSettingRep = globalSettingRep;
			_personRepository = personRepository;
		}

		public void Consume(CalculateBadgeMessage message)
		{
			var toggleCalculateBadgeWithRankEnabled = _toggleManager.IsEnabled(Toggles.Gamification_NewBadgeCalculation_31185);

			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat(
					"Consume CalculateBadgeMessage (Id=\"{0})\" with BusinessUnit=\"{1}\", DataSource=\"{2}\" and Timezone=\"{3}\"",
					message.Identity, message.BusinessUnitId, message.Datasource, message.TimeZoneCode);
			}

			if (_runningEtlJobChecker.NightlyEtlJobStillRunning())
			{
				var utcNow = DateTime.UtcNow;
				// If the ETL nightly job still running, then delay badge calculation 5 minutes later.
				if (logger.IsDebugEnabled)
				{
					logger.Debug("The \"Nightly\" ETL job is still running, will retry in 5 minutes.");
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

			using (var uow = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				var teamSettings = _teamSettingsRepository.FindAllTeamGamificationSettingsSortedByTeam().ToList();
				if (!teamSettings.Any())
				{
					//error happens
					logger.Info("No gamification setting applied to any team, no badge calculation will be done");
					resendMessage(message);
					return;
				}
				var settings = teamSettings.Select(t => t.GamificationSetting).Distinct();
				var today = _now.LocalDateOnly();
				var calculateDate = new DateOnly(message.CalculationDate);

				foreach (var setting in settings)
				{
					var settingId = setting.Id;
					if (setting.IsDeleted || (!setting.AHTBadgeEnabled && !setting.AdherenceBadgeEnabled && !setting.AnsweredCallsBadgeEnabled))
					{
						if (logger.IsDebugEnabled)
						{
							logger.DebugFormat(
								"No badge type is enabled or setting is deleted. nothing will be done for BusinessUnit=\"{0}\", DataSource=\"{1}\" and Timezone=\"{2}\""
								+ "(setting Id=\"{3}\", IsDeleted=\"{4}\")",
								message.BusinessUnitId, message.Datasource, message.TimeZoneCode, setting.Id, setting.IsDeleted);
						}
						continue;
					}

					var agentsWithSetting = new List<IPerson>();
					foreach (var teamSetting in teamSettings.Where(teamSetting => teamSetting.GamificationSetting.Id == settingId))
					{
						agentsWithSetting.AddRange(_personRepository.FindPeopleBelongTeam(teamSetting.Team,
							new DateOnlyPeriod(today.AddDays(-1), today.AddDays(1))));
					}
					agentsWithSetting = agentsWithSetting.Distinct().ToList();

					var isRuleWithDifferentThreshold = setting.GamificationSettingRuleSet ==
													   GamificationSettingRuleSet.RuleWithDifferentThreshold;

					if (setting.AdherenceBadgeEnabled)
					{
						var adherenceReportSetting = _globalSettingRep.FindValueByKey(AdherenceReportSetting.Key,
							new AdherenceReportSetting());

						if (toggleCalculateBadgeWithRankEnabled && isRuleWithDifferentThreshold)
						{
							var newAwardedBadgesWithRankForAdherence =
								_badgeWithRankCalculator.CalculateAdherenceBadges(agentsWithSetting, message.TimeZoneCode, calculateDate,
									adherenceReportSetting.CalculationMethod, setting, message.BusinessUnitId).ToList();
							if (logger.IsDebugEnabled)
							{
								logger.DebugFormat("Total {0} agents will get new badge for adherence",
									newAwardedBadgesWithRankForAdherence.Count);
							}
							sendMessagesToPeopleGotABadge(newAwardedBadgesWithRankForAdherence, setting, calculateDate, BadgeType.Adherence);
						}
						else
						{
							var newAwardedBadgesForAdherence =
								_calculator.CalculateAdherenceBadges(agentsWithSetting, message.TimeZoneCode, calculateDate,
									adherenceReportSetting.CalculationMethod, setting, message.BusinessUnitId).ToList();
							if (logger.IsDebugEnabled)
							{
								logger.DebugFormat("Total {0} agents will get new badge for adherence", newAwardedBadgesForAdherence.Count);
							}
							sendMessagesToPeopleGotABadge(newAwardedBadgesForAdherence, setting, calculateDate, BadgeType.Adherence);
						}
					}

					if (setting.AHTBadgeEnabled)
					{
						if (toggleCalculateBadgeWithRankEnabled && isRuleWithDifferentThreshold)
						{
							var newAwardedBadgesWithRankForAht =
								_badgeWithRankCalculator.CalculateAHTBadges(agentsWithSetting, message.TimeZoneCode, calculateDate, setting,
									message.BusinessUnitId).ToList();
							if (logger.IsDebugEnabled)
							{
								logger.DebugFormat("Total {0} agents will get new badge for AHT", newAwardedBadgesWithRankForAht.Count);
							}
							sendMessagesToPeopleGotABadge(newAwardedBadgesWithRankForAht, setting, calculateDate,
								BadgeType.AverageHandlingTime);
						}
						else
						{
							var newAwardedBadgesForAht =
								_calculator.CalculateAHTBadges(agentsWithSetting, message.TimeZoneCode, calculateDate, setting,
									message.BusinessUnitId).ToList();
							if (logger.IsDebugEnabled)
							{
								logger.DebugFormat("Total {0} agents will get new badge for AHT", newAwardedBadgesForAht.Count);
							}
							sendMessagesToPeopleGotABadge(newAwardedBadgesForAht, setting, calculateDate, BadgeType.AverageHandlingTime);
						}
					}

					if (setting.AnsweredCallsBadgeEnabled)
					{
						if (toggleCalculateBadgeWithRankEnabled && isRuleWithDifferentThreshold)
						{
							var newAwardedBadgesWithRankForAnsweredCalls =
								_badgeWithRankCalculator.CalculateAnsweredCallsBadges(agentsWithSetting, message.TimeZoneCode, calculateDate,
									setting, message.BusinessUnitId)
									.ToList();
							if (logger.IsDebugEnabled)
							{
								logger.DebugFormat("Total {0} agents will get new badge for answered calls",
									newAwardedBadgesWithRankForAnsweredCalls.Count);
							}
							sendMessagesToPeopleGotABadge(newAwardedBadgesWithRankForAnsweredCalls, setting, calculateDate,
								BadgeType.AnsweredCalls);
						}
						else
						{
							var newAwardedBadgesForAnsweredCalls =
								_calculator.CalculateAnsweredCallsBadges(agentsWithSetting, message.TimeZoneCode,
									calculateDate, setting, message.BusinessUnitId).ToList();
							if (logger.IsDebugEnabled)
							{
								logger.DebugFormat("Total {0} agents will get new badge for answered calls",
									newAwardedBadgesForAnsweredCalls.Count);
							}
							sendMessagesToPeopleGotABadge(newAwardedBadgesForAnsweredCalls, setting, calculateDate, BadgeType.AnsweredCalls);
						}
					}
					uow.PersistAll();
				}

				if (_serviceBus == null) return;
				resendMessage(message);

			}
		}

		private void resendMessage(CalculateBadgeMessage message, DateTime resendTime)
		{
			var newMessage = new CalculateBadgeMessage
			{
				Datasource = message.Datasource,
				BusinessUnitId = message.BusinessUnitId,
				Timestamp = DateTime.UtcNow,
				TimeZoneCode = message.TimeZoneCode,
				CalculationDate = message.CalculationDate.AddDays(1)
			};

			_serviceBus.DelaySend(resendTime, newMessage);

			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat(
					"Delay Sending CalculateBadgeMessage (Id=\"{0}\") to Service Bus for Timezone=\"{1}\" on next calculation time={2:yyyy-MM-dd HH:mm:ss}",
					newMessage.Identity, newMessage.TimeZoneCode, resendTime);
			}
		}

		private void resendMessage(CalculateBadgeMessage message)
		{
			var today = _now.LocalDateOnly();
			var tomorrow = new DateTime(today.AddDays(1).Date.Ticks, DateTimeKind.Unspecified);
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(message.TimeZoneCode);

			// Set badge calculation start at 5:00 AM
			// Just hard code it now, the best solution is to trigger it from ETL
			var nextMessageShouldBeProcessed = TimeZoneInfo.ConvertTime(tomorrow.AddHours(5), timeZone, TimeZoneInfo.Local);

			resendMessage(message, nextMessageShouldBeProcessed);
		}

		private void sendMessagesToPeopleGotABadge(IEnumerable<IAgentBadgeTransaction> newAwardedBadges,
			IGamificationSetting setting, DateOnly calculateDate, BadgeType badgeType)
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
						setting.SilverToBronzeBadgeRate * setting.GoldToSilverBadgeRate);

					sendBadgeMessage(person, badgeType, badgeRank, message);
				}
			}
		}

		private void sendMessagesToPeopleGotABadge(IEnumerable<IAgentBadgeWithRankTransaction> newAwardedBadges,
			IGamificationSetting setting, DateOnly calculateDate, BadgeType badgeType)
		{
			var agentBadgeWithRankTransactions = newAwardedBadges as IList<IAgentBadgeWithRankTransaction> ??
												 newAwardedBadges.ToList();

			var existedBadges =
				_badgeWithRankRepository.Find(agentBadgeWithRankTransactions.Select(x => x.Person.Id.GetValueOrDefault()), badgeType);
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
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Send {3} badge text message to agent {0} (ID: {1}) for badge type: {2}", person.Name, person.Id,
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
