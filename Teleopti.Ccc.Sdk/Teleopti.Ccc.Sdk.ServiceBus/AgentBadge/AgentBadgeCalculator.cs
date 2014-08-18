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

			if (agentsThatShouldGetBadge != null)
			{
				foreach (
					var person in
						agentsThatShouldGetBadge.Select(agent => allPersons.Single(x => x.Id != null && x.Id.Value == agent))
							.Where(a => a != null))
				{
					if (Logger.IsDebugEnabled)
					{
						Logger.DebugFormat("Award badge to agent {0} (ID: {1}) for {2}", 
							person.Name, person.Id, badgeType);
					}

					var badge = person.Badges.Any(x => x.BadgeType == badgeType)
						? person.Badges.Single(x => x.BadgeType == badgeType)
						: null;

					if (badge == null || badge.LastCalculatedDate < date)
					{
						person.AddBadge(new Domain.Common.AgentBadge
						{
							BronzeBadge = 1,
							BadgeType = badgeType,
							LastCalculatedDate = date
						}, silverToBronzeBadgeRate, goldToSilverBadgeRate);
						personsThatGotABadge.Add(person);
					}

					if (Logger.IsDebugEnabled)
					{
						badge = person.Badges.First(x => x.BadgeType == badgeType);
						Logger.DebugFormat(
							"Now the agent {0} (ID: {1}) has {2} bronze badge, {3} silver badge, {4} gold badge "
							+ "for {5}", person.Name, person.Id, badge.BronzeBadge, badge.SilverBadge, badge.GoldBadge,
							badgeType);
					}
				}
			}

			return personsThatGotABadge;
		}

		public IEnumerable<IPerson> Calculate(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, AdherenceReportSettingCalculationMethod adherenceCalculationMethod, IAgentBadgeThresholdSettings setting)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat(
					"Calculate badge for timezone: {0}, date: {1}, AdherenceReportSettingCalculationMethod: {2},"
					+ "silver to bronze badge rate: {3}, gold to silver badge rate: {4}", timezoneCode, date.Date,
					adherenceCalculationMethod, setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate);
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

			agentsList = _statisticRepository.LoadAgentsOverThresholdForAnsweredCalls(timezoneCode, date.Date, setting.AnsweredCallsThreshold);
			agents = agentsList == null ? new List<Guid>() : agentsList.ToList();

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

			agentsList = _statisticRepository.LoadAgentsUnderThresholdForAHT(timezoneCode, date.Date, setting.AHTThreshold);
			agents = agentsList == null ? new List<Guid>() : agentsList.ToList();

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
				Logger.DebugFormat("Total {0} agents will get new badge", personsThatGotBadge.Count);
			}

			return personsThatGotBadge;
		}
	}
}
