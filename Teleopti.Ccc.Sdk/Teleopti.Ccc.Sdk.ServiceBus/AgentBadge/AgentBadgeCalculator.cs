﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class AgentBadgeCalculator : IAgentBadgeCalculator
	{
		private readonly IStatisticRepository _statisticRepository;

		public AgentBadgeCalculator(IStatisticRepository statisticRepository)
		{
			_statisticRepository = statisticRepository;
		}

		protected IList<IPerson> AddBadge(IEnumerable<IPerson> allPersons, IEnumerable<Guid> agentsThatShouldGetBadge,
			BadgeType badgeType,
			int silverToBronzeBadgeRate, int goldToSilverBadgeRate)
		{
			var personsThatGotABadge = new List<IPerson>();
			if (agentsThatShouldGetBadge != null)
			{
				foreach (
					var person in
						agentsThatShouldGetBadge.Select(agent => allPersons.Single(x => x.Id != null && x.Id.Value == agent))
							.Where(a => a != null))
				{
					person.AddBadge(new Domain.Common.AgentBadge
					{
						BronzeBadge = 1,
						BadgeType = badgeType
					});

					var badge = person.Badges.Single(x => x.BadgeType == badgeType);
					if (badge.BronzeBadge >= silverToBronzeBadgeRate)
					{
						badge.SilverBadge = badge.SilverBadge + badge.BronzeBadge/silverToBronzeBadgeRate;
						badge.BronzeBadge = badge.BronzeBadge%silverToBronzeBadgeRate;
					}

					if (badge.SilverBadge >= goldToSilverBadgeRate)
					{
						badge.GoldBadge = badge.GoldBadge + badge.SilverBadge/goldToSilverBadgeRate;
						badge.SilverBadge = badge.SilverBadge%goldToSilverBadgeRate;
					}

					personsThatGotABadge.Add(person);
				}
			}

			return personsThatGotABadge;
		}

		public IEnumerable<IPerson> Calculate(IStatelessUnitOfWork unitOfWork, IEnumerable<IPerson> allPersons, int timezoneId,
			DateTime date, AdherenceReportSettingCalculationMethod adherenceCalculationMethod, int silverToBronzeBadgeRate, int goldToSilverBadgeRate)
		{
			var personsThatGotBadge = new List<IPerson>();
			var agents = _statisticRepository.LoadAgentsOverThresholdForAdherence(unitOfWork, adherenceCalculationMethod, timezoneId, date);
			if (agents != null)
			{
				var personsThatGotAAdherenceBadge = AddBadge(allPersons, agents, BadgeType.Adherence, silverToBronzeBadgeRate, goldToSilverBadgeRate);
				personsThatGotBadge.AddRange(personsThatGotAAdherenceBadge);
			}

			agents = _statisticRepository.LoadAgentsOverThresholdForAnsweredCalls(unitOfWork, timezoneId, date);
			if (agents != null)
			{
				var personsThatGotAAnsweredCallsBadge = AddBadge(allPersons, agents, BadgeType.AnsweredCalls, silverToBronzeBadgeRate, goldToSilverBadgeRate);
				personsThatGotBadge.AddRange(personsThatGotAAnsweredCallsBadge);
			}

			agents = _statisticRepository.LoadAgentsUnderThresholdForAHT(unitOfWork, timezoneId, date);
			if (agents != null)
			{
				var personsThatGotAAHTBadge = AddBadge(allPersons, agents, BadgeType.AverageHandlingTime, silverToBronzeBadgeRate, goldToSilverBadgeRate);
				personsThatGotBadge.AddRange(personsThatGotAAHTBadge);
			}

			return personsThatGotBadge;
		}
	}
}
