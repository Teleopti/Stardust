using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public class CalculateBadges
	{
		private readonly ITeamGamificationSettingRepository _teamSettingsRepository;
		private readonly IPushMessagePersister _msgPersister;
		private readonly IAgentBadgeCalculator _calculator;
		private readonly IAgentBadgeWithRankCalculator _badgeWithRankCalculator;
		private readonly IAgentBadgeRepository _badgeRepository;
		private readonly IAgentBadgeWithRankRepository _badgeWithRankRepository;
		private static readonly ILog logger = LogManager.GetLogger(typeof (CalculateBadges));
		private readonly IGlobalSettingDataRepository _globalSettingRep;
		private readonly IPersonRepository _personRepository;

		public CalculateBadges(
			ITeamGamificationSettingRepository teamSettingsRepository,
			IPushMessagePersister msgRepository,
			IAgentBadgeCalculator calculator,
			IAgentBadgeWithRankCalculator badgeWithRankCalculator,
			IAgentBadgeRepository badgeRepository,
			IAgentBadgeWithRankRepository badgeWithRankRepository,
			IGlobalSettingDataRepository globalSettingRep,
			IPersonRepository personRepository)
		{
			_teamSettingsRepository = teamSettingsRepository;
			_msgPersister = msgRepository;
			_calculator = calculator;
			_badgeWithRankCalculator = badgeWithRankCalculator;
			_badgeRepository = badgeRepository;
			_badgeWithRankRepository = badgeWithRankRepository;
			_globalSettingRep = globalSettingRep;
			_personRepository = personRepository;
		}

		public void Calculate(CalculateBadgeMessage message)
		{
			var teamSettings = _teamSettingsRepository.FindAllTeamGamificationSettingsSortedByTeam().Where(t => t.GamificationSetting!=null).ToLookup(t => t.GamificationSetting);
			var settings = teamSettings.Select(t => t.Key);
			var calculateDate = new DateOnly(message.CalculationDate);

			foreach (var setting in settings)
			{
				if (setting.IsDeleted)
				{
					continue;
				}

				var agentsWithSetting = new List<IPerson>();
				foreach (var teamSetting in teamSettings[setting])
				{
					agentsWithSetting.AddRange(_personRepository.FindPeopleBelongTeam(teamSetting.Team, calculateDate.ToDateOnlyPeriod().Inflate(1)));
				}
				agentsWithSetting = agentsWithSetting.Distinct().ToList();

				var isRuleWithDifferentThreshold =
					setting.GamificationSettingRuleSet == GamificationSettingRuleSet.RuleWithDifferentThreshold;

				calculateAdherenceBadge(message, setting, isRuleWithDifferentThreshold, agentsWithSetting, calculateDate);
				calculateAhtBadge(message, setting, isRuleWithDifferentThreshold, agentsWithSetting, calculateDate);
				calculateAnsweredCallsBadge(message, setting, isRuleWithDifferentThreshold, agentsWithSetting, calculateDate);

				calculateBadges(setting, isRuleWithDifferentThreshold, agentsWithSetting, calculateDate);
			}
		}
		
		private void calculateBadges(IGamificationSetting setting,
			bool isRuleWithDifferentThreshold, IList<IPerson> agentsWithSetting, DateOnly calculateDate)
		{
			foreach (var badgeSetting in setting.BadgeSettings)
			{
				if (!badgeSetting.Enabled) continue;
				if (isRuleWithDifferentThreshold)
				{
					var agentBadgeWithRank = _badgeWithRankCalculator.CalculateBadges(agentsWithSetting, calculateDate, badgeSetting);
					sendMessage(agentBadgeWithRank, badgeSetting.Name, calculateDate);
				}
				else
				{
					var agentBadge = _calculator.CalculateBadges(agentsWithSetting, calculateDate, badgeSetting);
					sendMessage(agentBadge, badgeSetting, calculateDate, setting);
				}
			}
		}

		private void sendMessage(IEnumerable<IAgentBadgeWithRankTransaction> agentBadgeWithRankTransactions, string badgeName, DateOnly calculateDate)
		{
			foreach (var agentBadgeWithRankTransaction in agentBadgeWithRankTransactions)
			{
				var person = agentBadgeWithRankTransaction.Person;
				string message;
				MessageType messageType;
				if (agentBadgeWithRankTransaction.BronzeBadgeAmount > 0)
				{
					message = string.Format(Resources.YouGotANewBronzeBadge, badgeName, calculateDate.Date);
					messageType = MessageType.ExternalBronzeBadge;
				}
				else if (agentBadgeWithRankTransaction.SilverBadgeAmount > 0)
				{
					message = string.Format(Resources.YouGotANewSilverBadge, badgeName, calculateDate.Date);
					messageType = MessageType.ExternalSilverBadge;
				}
				else
				{
					message = string.Format(Resources.YouGotANewGoldBadge, badgeName, calculateDate.Date);
					messageType = MessageType.ExternalGoldBadge;
				}

				SendPushMessageService
					.CreateConversation(Resources.Congratulations, message, false, messageType)
					.To(person)
					.SendConversation(_msgPersister);
			}
		}

		private void sendMessage(IEnumerable<IAgentBadgeTransaction> agentBadgeTransactions, IBadgeSetting badgeSetting, DateOnly calculateDate, IGamificationSetting setting)
		{
			var existedBadges = (_badgeRepository.Find(agentBadgeTransactions.Select(x => x.Person.Id.GetValueOrDefault()),
									 badgeSetting.QualityId) ?? new AgentBadge[0]).ToLookup(b => b.Person);

			foreach (var agentBadgeTransaction in agentBadgeTransactions)
			{
				var person = agentBadgeTransaction.Person;
				var existedBadge = existedBadges[person.Id.Value].SingleOrDefault() ?? new AgentBadge
				{
					Person = person.Id.GetValueOrDefault(),
					TotalAmount = 0,
					BadgeType = badgeSetting.QualityId
				};

				existedBadge.TotalAmount += agentBadgeTransaction.Amount;

				var message = "";
				var messageType = MessageType.Information;
				if (existedBadge.IsBronzeBadgeAdded(setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate))
				{
					message = string.Format(Resources.YouGotANewBronzeBadge, badgeSetting.Name, calculateDate);
					messageType = MessageType.ExternalBronzeBadge;
				}

				if (existedBadge.IsSilverBadgeAdded(setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate))
				{
					message = string.Format(Resources.YouGotANewSilverBadge, badgeSetting.Name, calculateDate);
					messageType = MessageType.ExternalSilverBadge;
				}

				if (existedBadge.IsGoldBadgeAdded(setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate))
				{
					message = string.Format(Resources.YouGotANewGoldBadge, badgeSetting.Name, calculateDate);
					messageType = MessageType.ExternalGoldBadge;
				}

				SendPushMessageService
				.CreateConversation(Resources.Congratulations, message, false, messageType)
				.To(person)
				.SendConversation(_msgPersister);
			}
		}

		private void calculateAdherenceBadge(CalculateBadgeMessage message, IGamificationSetting setting,
			bool isRuleWithDifferentThreshold, IEnumerable<IPerson> agentsWithSetting, DateOnly calculateDate)
		{
			if (!setting.AdherenceBadgeEnabled) return;

			var adherenceReportSetting = _globalSettingRep.FindValueByKey(AdherenceReportSetting.Key,
				new AdherenceReportSetting());

			if (isRuleWithDifferentThreshold)
			{
				var newAwardedBadgesWithRankForAdherence =
					_badgeWithRankCalculator.CalculateAdherenceBadges(agentsWithSetting, message.TimeZoneCode, calculateDate,
						adherenceReportSetting.CalculationMethod, setting, message.LogOnBusinessUnitId).ToList();
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
						adherenceReportSetting.CalculationMethod, setting, message.LogOnBusinessUnitId).ToList();
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Total {0} agents will get new badge for adherence", newAwardedBadgesForAdherence.Count);
				}
				sendMessagesToPeopleGotABadge(newAwardedBadgesForAdherence, setting, calculateDate, BadgeType.Adherence);
			}
		}

		private void calculateAhtBadge(CalculateBadgeMessage message, IGamificationSetting setting,
			bool isRuleWithDifferentThreshold, IEnumerable<IPerson> agentsWithSetting, DateOnly calculateDate)
		{
			if (!setting.AHTBadgeEnabled) return;

			if (isRuleWithDifferentThreshold)
			{
				var newAwardedBadgesWithRankForAht =
					_badgeWithRankCalculator.CalculateAHTBadges(agentsWithSetting, message.TimeZoneCode, calculateDate, setting,
						message.LogOnBusinessUnitId).ToList();
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
						message.LogOnBusinessUnitId).ToList();
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Total {0} agents will get new badge for AHT", newAwardedBadgesForAht.Count);
				}
				sendMessagesToPeopleGotABadge(newAwardedBadgesForAht, setting, calculateDate, BadgeType.AverageHandlingTime);
			}
		}

		private void calculateAnsweredCallsBadge(CalculateBadgeMessage message, IGamificationSetting setting,
			bool isRuleWithDifferentThreshold, IEnumerable<IPerson> agentsWithSetting, DateOnly calculateDate)
		{
			if (!setting.AnsweredCallsBadgeEnabled) return;

			if (isRuleWithDifferentThreshold)
			{
				var newAwardedBadgesWithRankForAnsweredCalls =
					_badgeWithRankCalculator.CalculateAnsweredCallsBadges(agentsWithSetting, message.TimeZoneCode, calculateDate,
						setting, message.LogOnBusinessUnitId).ToList();
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
						calculateDate, setting, message.LogOnBusinessUnitId).ToList();
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Total {0} agents will get new badge for answered calls",
						newAwardedBadgesForAnsweredCalls.Count);
				}
				sendMessagesToPeopleGotABadge(newAwardedBadgesForAnsweredCalls, setting, calculateDate, BadgeType.AnsweredCalls);
			}
		}

		private void sendMessagesToPeopleGotABadge(IEnumerable<IAgentBadgeTransaction> newAwardedBadges,
			IGamificationSetting setting, DateOnly calculateDate, int badgeType)
		{
			var agentBadgeTransactions = newAwardedBadges as IList<IAgentBadgeTransaction> ?? newAwardedBadges.ToList();

			var existedBadges = (_badgeRepository.Find(agentBadgeTransactions.Select(x => x.Person.Id.GetValueOrDefault()),
				badgeType) ?? new AgentBadge[0]).ToLookup(b => b.Person);
			foreach (var badgeTransaction in agentBadgeTransactions)
			{
				var person = badgeTransaction.Person;

				var existedBadge = existedBadges[person.Id.Value].SingleOrDefault() ?? new Common.AgentBadge
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
			IGamificationSetting setting, DateOnly calculateDate, int badgeType)
		{
			var agentBadgeWithRankTransactions = newAwardedBadges as IList<IAgentBadgeWithRankTransaction> ??
												 newAwardedBadges.ToList();

			var existedBadges =
				(_badgeWithRankRepository.Find(agentBadgeWithRankTransactions.Select(x => x.Person.Id.GetValueOrDefault()), badgeType) ?? new IAgentBadgeWithRank[0]).ToLookup(b => b.Person);
			foreach (var badgeTransaction in agentBadgeWithRankTransactions)
			{
				var person = badgeTransaction.Person;

				var existedBadge = existedBadges[person.Id.Value].SingleOrDefault() ?? new AgentBadgeWithRank
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

		private MessageType getMessageType(int badgeType, BadgeRank badgeRank)
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

		private void sendBadgeMessage(IPerson person, int badgeType, BadgeRank badgeRank, string message)
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