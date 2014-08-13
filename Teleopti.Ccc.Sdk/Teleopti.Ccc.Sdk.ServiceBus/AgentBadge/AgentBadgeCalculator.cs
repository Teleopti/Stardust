﻿using System;
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

					IAgentBadge badge;
					if (!person.Badges.Any(x => x.BadgeType == badgeType))
					{
						badge = null;
					}
					else
					{
						badge = person.Badges.First(x => x.BadgeType == badgeType);
					}

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
						Logger.DebugFormat("Now the agent {0} (ID: {1}) has {2} bronze badge, {3} silver badge, {4} gold badge "
						                   + "for {5}", person.Name, person.Id, badge.BronzeBadge, badge.SilverBadge, badge.GoldBadge,
							badgeType);
					}
				}
			}

			return personsThatGotABadge;
		}

		public IEnumerable<IPerson> Calculate(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, AdherenceReportSettingCalculationMethod adherenceCalculationMethod, int silverToBronzeBadgeRate, int goldToSilverBadgeRate)
		{
			if (Logger.IsDebugEnabled)
			{
				Logger.DebugFormat("Calculate badge for timezone: {0}, date: {1}, AdherenceReportSettingCalculationMethod: {2},"
				                   + "silver to bronze badge rate: {3}, gold to silver badge rate: {4}", timezoneCode, date.Date,
					adherenceCalculationMethod, silverToBronzeBadgeRate, goldToSilverBadgeRate);
			}

			var personsThatGotBadge = new List<IPerson>();
			var agents = _statisticRepository.LoadAgentsOverThresholdForAdherence(adherenceCalculationMethod, timezoneCode, date.Date);

			if (agents != null && agents.Any())
			{
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("{0} agents will get badge for adherence", agents.Count());
				}

				var personsThatGotAAdherenceBadge = AddBadge(allPersons, agents, BadgeType.Adherence, silverToBronzeBadgeRate,
					goldToSilverBadgeRate, date);
				personsThatGotBadge.AddRange(personsThatGotAAdherenceBadge);
			}
			else
			{
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("No agents will get badge for adherence");
				}
			}

			agents = _statisticRepository.LoadAgentsOverThresholdForAnsweredCalls(timezoneCode, date.Date);

			if (agents != null && agents.Any())
			{
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("{0} agents will get badge for answered calls", agents.Count());
				}

				var personsThatGotAAnsweredCallsBadge = AddBadge(allPersons, agents, BadgeType.AnsweredCalls, silverToBronzeBadgeRate, goldToSilverBadgeRate, date);
				personsThatGotBadge.AddRange(personsThatGotAAnsweredCallsBadge);
			}
			else
			{
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("No agents will get badge for answered calls");
				}
			}

			agents = _statisticRepository.LoadAgentsUnderThresholdForAHT(timezoneCode, date.Date);
			if (agents != null && agents.Any())
			{
				if (Logger.IsDebugEnabled)
				{
					Logger.DebugFormat("{0} agents will get badge for AHT", agents.Count());
				}

				var personsThatGotAAHTBadge = AddBadge(allPersons, agents, BadgeType.AverageHandlingTime, silverToBronzeBadgeRate, goldToSilverBadgeRate, date);
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
