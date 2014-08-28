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
		private static readonly ILog Logger = LogManager.GetLogger(typeof(AgentBadgeCalculator));
		private readonly IStatisticRepository _statisticRepository;

		public AgentBadgeCalculator(IStatisticRepository statisticRepository)
		{
			_statisticRepository = statisticRepository;
		}

		protected IList<IPerson> AddBadge(IEnumerable<IPerson> allPersons, IEnumerable<Guid> agentsThatShouldGetBadge,
			BadgeType badgeType, int silverToBronzeBadgeRate, int goldToSilverBadgeRate, DateOnly date)
		{
			var personsThatGotABadge = new List<IPerson>();

			if (agentsThatShouldGetBadge == null)
			{
				return personsThatGotABadge;
			}

			foreach (
				var person in
					agentsThatShouldGetBadge.Select(agent => allPersons.Single(x => x.Id != null && x.Id.Value == agent))
						.Where(a => a != null))
			{
				const string badgeDescriptionTemplate = "{0} bronze badge(s), {1} silver badge(s) and {2} gold badge(s) for {3}";
				var badge = person.Badges.SingleOrDefault(x => x.BadgeType == badgeType);

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
						BronzeBadge = 1,
						BronzeBadgeAdded = true,
						BadgeType = badgeType,
						LastCalculatedDate = date
					};
					person.AddBadge(newBadge, silverToBronzeBadgeRate, goldToSilverBadgeRate);
					personsThatGotABadge.Add(person);

					if (Logger.IsDebugEnabled)
					{
						badge = person.Badges.First(x => x.BadgeType == badgeType);
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

			return personsThatGotABadge;
		}

		public IEnumerable<IPerson> CalculateAdherenceBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
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
			var personsThatGotBadge = new List<IPerson>();
			var agentsList =
				_statisticRepository.LoadAgentsOverThresholdForAdherence(adherenceCalculationMethod, timezoneCode, date.Date, setting.AdherenceThreshold);

			var agents = agentsList == null ? new List<Guid>() : agentsList.ToList();
			if (agents.Any())
			{
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("{0} agents will get badge for adherence", agents.Count());
				}

				var personsThatGotAAdherenceBadge = AddBadge(personList, agents, BadgeType.Adherence, setting.SilverToBronzeBadgeRate,
					setting.GoldToSilverBadgeRate, date);
				personsThatGotBadge.AddRange(personsThatGotAAdherenceBadge);
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
				Logger.DebugFormat("Total {0} new badge(s) been awarded to agents for adherence.", personsThatGotBadge.Count);
			}

			return personsThatGotBadge;
		}

		public IEnumerable<IPerson> CalculateAHTBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
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
			var personsThatGotBadge = new List<IPerson>();
			var agentsList = _statisticRepository.LoadAgentsUnderThresholdForAHT(timezoneCode, date.Date, setting.AHTThreshold);
			var agents = agentsList == null ? new List<Guid>() : agentsList.ToList();

			if (agents.Any())
			{
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("{0} agents will get badge for AHT", agents.Count());
				}

				var personsThatGotAAHTBadge
					= AddBadge(personList, agents, BadgeType.AverageHandlingTime, setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate, date);
				personsThatGotBadge.AddRange(personsThatGotAAHTBadge);
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
				Logger.DebugFormat("Total {0} new badge(s) been awarded to agents for AHT.", personsThatGotBadge.Count);
			}

			return personsThatGotBadge;
		}

		public IEnumerable<IPerson> CalculateAnsweredCallsBadges(IEnumerable<IPerson> allPersons, string timezoneCode,
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
			var personsThatGotBadge = new List<IPerson>();
			var agentsList = _statisticRepository.LoadAgentsOverThresholdForAnsweredCalls(timezoneCode, date.Date, setting.AnsweredCallsThreshold);
			var agents = agentsList == null ? new List<Guid>() : agentsList.ToList();

			if (agents.Any())
			{
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("{0} agents will get badge for answered calls", agents.Count());
				}

				var personsThatGotAAnsweredCallsBadge
					= AddBadge(personList, agents, BadgeType.AnsweredCalls, setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate, date);
				personsThatGotBadge.AddRange(personsThatGotAAnsweredCallsBadge);
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
				Logger.DebugFormat("Total {0} new badge(s) been awarded to agents for answered calls.", personsThatGotBadge.Count);
			}

			return personsThatGotBadge;
		}
	}
}
