using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class AgentBadgeCalculator : IAgentBadgeCalculator
	{
		const string badgeDescriptionTemplate = "{0} bronze badge(s), {1} silver badge(s) and {2} gold badge(s) for {3}";

		private static readonly ILog Logger = LogManager.GetLogger(typeof(AgentBadgeCalculator));
		private readonly IStatisticRepository _statisticRepository;
		private readonly IAgentBadgeRepository _badgeRepository;

		public AgentBadgeCalculator(IStatisticRepository statisticRepository, IAgentBadgeRepository badgeRepository)
		{
			_statisticRepository = statisticRepository;
			_badgeRepository = badgeRepository;
		}

		protected IList<IAgentBadge> AddBadge(IEnumerable<IPerson> allPersons, IEnumerable<Guid> agentsThatShouldGetBadge,
			BadgeType badgeType, int silverToBronzeBadgeRate, int goldToSilverBadgeRate, DateOnly date)
		{
			var newAwardedBadges = new List<IAgentBadge>();

			var agentsListShouldGetBadge = agentsThatShouldGetBadge as IList<Guid> ?? agentsThatShouldGetBadge.ToList();
			if (!agentsListShouldGetBadge.Any())
			{
				return newAwardedBadges;
			}

			foreach (
				var person in
					agentsListShouldGetBadge.Select(agent => allPersons.Single(x => x.Id != null && x.Id.Value == agent))
						.Where(a => a != null))
			{
				var badge = _badgeRepository.Find(person, badgeType);

				if (badge == null || badge.LastCalculatedDate < date)
				{
					if (Logger.IsDebugEnabled)
					{
						if (badge == null)
						{
							Logger.DebugFormat("Award badge to agent {0} (ID: {1}), this agent has no badge for {2} yet.",
								person.Name, person.Id, badgeType);
						}
						else
						{
							var badgeDesc = string.Format(badgeDescriptionTemplate, badge.BronzeBadge, badge.SilverBadge, badge.GoldBadge,
								badgeType);
							Logger.DebugFormat(
								"Award badge to agent {0} (ID: {1}), Now this agent has {2}.", person.Name, person.Id, badgeDesc);
						}
					}

					var newBadge = new Domain.Common.AgentBadge
					{
						Person = person,
						BronzeBadge = 1,
						BronzeBadgeAdded = true,
						BadgeType = badgeType,
						LastCalculatedDate = date
					};
					if (badge == null)
					{
						badge = newBadge;
						_badgeRepository.Add(badge);
					}
					else
					{
						badge.AddBadge(newBadge, silverToBronzeBadgeRate, goldToSilverBadgeRate);
					}

					newAwardedBadges.Add(badge);

					if (Logger.IsDebugEnabled)
					{
						var badgeDesc = string.Format(badgeDescriptionTemplate, badge.BronzeBadge, badge.SilverBadge, badge.GoldBadge,
							badgeType);
						Logger.DebugFormat(
							"Now the agent {0} (ID: {1}) has {2}.", person.Name, person.Id, badgeDesc);
					}
				}
				else
				{
					if (Logger.IsDebugEnabled)
					{
						var badgeDesc = string.Format(badgeDescriptionTemplate, badge.BronzeBadge, badge.SilverBadge, badge.GoldBadge,
							badgeType);
						Logger.DebugFormat(
							"Last {0} badge calculated date {1:yyyy-MM-dd} of agent {2} (ID: {3}) is not earlier than {4:yyyy-MM-dd}, "
							+ "no badge will awarded. now this agent has {5}.",
							badgeType, badge.LastCalculatedDate.Date, person.Name, person.Id, date.Date, badgeDesc);
					}
				}
			}

			return newAwardedBadges;
		}

		public IEnumerable<IAgentBadge> CalculateAdherenceBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, AdherenceReportSettingCalculationMethod adherenceCalculationMethod,
			IAgentBadgeThresholdSettings setting)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat(
					"Calculate adherence badges for timezone: {0}, date: {1:yyyy-MM-dd HH:mm:ss}, AdherenceReportSettingCalculationMethod: {2},"
					+ "silver to bronze badge rate: {3}, gold to silver badge rate: {4}, adherenceThreshold: {5}", timezoneCode, date.Date,
					adherenceCalculationMethod, setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate, setting.AdherenceThreshold);
			}

			var personList = allPersons.ToList();
			var newAwardedBadges = new List<IAgentBadge>();
			var agentsList =
				_statisticRepository.LoadAgentsOverThresholdForAdherence(adherenceCalculationMethod, timezoneCode, date.Date, setting.AdherenceThreshold);

			var agents = agentsList == null ? new List<Guid>() : agentsList.ToList();
			if (agents.Any())
			{
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("{0} agents will get badge for adherence", agents.Count());
				}

				var newAwardedBadge = AddBadge(personList, agents, BadgeType.Adherence, setting.SilverToBronzeBadgeRate,
					setting.GoldToSilverBadgeRate, date);
				newAwardedBadges.AddRange(newAwardedBadge);
			}
			else
			{
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("No agents will get badge for adherence");
				}
			}

			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Total {0} new badge(s) been awarded to agents for adherence.", newAwardedBadges.Count);
			}

			return newAwardedBadges;
		}

		public IEnumerable<IAgentBadge> CalculateAHTBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, IAgentBadgeThresholdSettings setting)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat(
					"Calculate AHT badges for timezone: {0}, date: {1:yyyy-MM-dd HH:mm:ss},"
					+ "silver to bronze badge rate: {2}, gold to silver badge rate: {3}, AHT threshold: {4}.", timezoneCode, date.Date,
					 setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate, setting.AHTThreshold);
			}

			var personList = allPersons.ToList();
			var newAwardedBadges = new List<IAgentBadge>();
			var agentsList = _statisticRepository.LoadAgentsUnderThresholdForAHT(timezoneCode, date.Date, setting.AHTThreshold);
			var agents = agentsList == null ? new List<Guid>() : agentsList.ToList();

			if (agents.Any())
			{
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("{0} agents will get badge for AHT", agents.Count());
				}

				var newAwardedAHTBadges
					= AddBadge(personList, agents, BadgeType.AverageHandlingTime, setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate, date);
				newAwardedBadges.AddRange(newAwardedAHTBadges);
			}
			else
			{
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("No agents will get badge for AHT");
				}
			}
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Total {0} new badge(s) been awarded to agents for AHT.", newAwardedBadges.Count);
			}

			return newAwardedBadges;
		}

		public IEnumerable<IAgentBadge> CalculateAnsweredCallsBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, IAgentBadgeThresholdSettings setting)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat(
					"Calculate answered calls badges for timezone: {0}, date: {1:yyyy-MM-dd HH:mm:ss},"
					+ "silver to bronze badge rate: {2}, gold to silver badge rate: {3}, answeredCallsThreshold: {4}", timezoneCode, date.Date,
					 setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate, setting.AnsweredCallsThreshold);
			}

			var personList = allPersons.ToList();
			var newAwardedBadges = new List<IAgentBadge>();
			var agentsList = _statisticRepository.LoadAgentsOverThresholdForAnsweredCalls(timezoneCode, date.Date, setting.AnsweredCallsThreshold);
			var agents = agentsList == null ? new List<Guid>() : agentsList.ToList();

			if (agents.Any())
			{
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("{0} agents will get badge for answered calls", agents.Count());
				}

				var newAwardedAnsweredCallsBadges
					= AddBadge(personList, agents, BadgeType.AnsweredCalls, setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate, date);
				newAwardedBadges.AddRange(newAwardedAnsweredCallsBadges);
			}
			else
			{
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("No agents will get badge for answered calls");
				}
			}

			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Total {0} new badge(s) been awarded to agents for answered calls.", newAwardedBadges.Count);
			}

			return newAwardedBadges;
		}
	}
}
