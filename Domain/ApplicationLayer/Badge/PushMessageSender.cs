using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public class PushMessageSender : IPushMessageSender
	{
		private readonly IPushMessagePersister _msgPersister;
		private readonly IAgentBadgeRepository _badgeRepository;
		private readonly IAgentBadgeWithRankRepository _badgeWithRankRepository;

		public PushMessageSender(IPushMessagePersister msgPersister, IAgentBadgeRepository badgeRepository, IAgentBadgeWithRankRepository badgeWithRankRepository)
		{
			_msgPersister = msgPersister;
			_badgeRepository = badgeRepository;
			_badgeWithRankRepository = badgeWithRankRepository;
		}

		public void SendMessage(IEnumerable<IAgentBadgeWithRankTransaction> agentBadgeWithRankTransactions, IBadgeSetting badgeSetting, DateOnly calculateDate)
		{
			foreach (var agentBadgeWithRankTransaction in agentBadgeWithRankTransactions)
			{
				var person = agentBadgeWithRankTransaction.Person;
				string message;
				MessageType messageType;
				var culture = person.PermissionInformation.Culture();
				var date = calculateDate.Date.ToString(culture.DateTimeFormat.ShortDatePattern, culture);
				if (agentBadgeWithRankTransaction.BronzeBadgeAmount > 0)
				{
					message = string.Format(Resources.YouGotANewBronzeBadge, badgeSetting.Name, date);
					messageType = MessageType.ExternalBronzeBadge;
				}
				else if (agentBadgeWithRankTransaction.SilverBadgeAmount > 0)
				{
					message = string.Format(Resources.YouGotANewSilverBadge, badgeSetting.Name, date);
					messageType = MessageType.ExternalSilverBadge;
				}
				else
				{
					message = string.Format(Resources.YouGotANewGoldBadge, badgeSetting.Name, date);
					messageType = MessageType.ExternalGoldBadge;
				}

				SendPushMessageService
					.CreateConversation(Resources.Congratulations, message, false, messageType)
					.To(person)
					.SendConversation(_msgPersister);
			}
		}

		public void SendMessage(IEnumerable<IAgentBadgeTransaction> agentBadgeTransactions, IBadgeSetting badgeSetting, DateOnly calculateDate, IGamificationSetting setting)
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
				var culture = person.PermissionInformation.Culture();
				var date = calculateDate.Date.ToString(culture.DateTimeFormat.ShortDatePattern, culture);
				if (existedBadge.IsBronzeBadgeAdded(setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate))
				{
					message = string.Format(Resources.YouGotANewBronzeBadge, badgeSetting.Name, date);
					messageType = MessageType.ExternalBronzeBadge;
				}

				if (existedBadge.IsSilverBadgeAdded(setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate))
				{
					message = string.Format(Resources.YouGotANewSilverBadge, badgeSetting.Name, date);
					messageType = MessageType.ExternalSilverBadge;
				}

				if (existedBadge.IsGoldBadgeAdded(setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate))
				{
					message = string.Format(Resources.YouGotANewGoldBadge, badgeSetting.Name, date);
					messageType = MessageType.ExternalGoldBadge;
				}

				SendPushMessageService
					.CreateConversation(Resources.Congratulations, message, false, messageType)
					.To(person)
					.SendConversation(_msgPersister);
			}
		}

		public void SendMessagesToPeopleGotABadge(IEnumerable<IAgentBadgeTransaction> newAwardedBadges, IGamificationSetting setting, DateOnly calculateDate, int badgeType)
		{
			var agentBadgeTransactions = newAwardedBadges as IList<IAgentBadgeTransaction> ?? newAwardedBadges.ToList();

			var existedBadges = (_badgeRepository.Find(agentBadgeTransactions.Select(x => x.Person.Id.GetValueOrDefault()),
				badgeType) ?? new AgentBadge[0]).ToLookup(b => b.Person);
			foreach (var badgeTransaction in agentBadgeTransactions)
			{
				var person = badgeTransaction.Person;

				var existedBadge = existedBadges[person.Id.Value].SingleOrDefault() ?? new AgentBadge
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

		public void SendMessagesToPeopleGotABadge(IEnumerable<IAgentBadgeWithRankTransaction> newAwardedBadges,
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
