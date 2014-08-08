using System;
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
			int silverToBronzeBadgeRate, int goldToSilverBadgeRate, DateOnly date)
		{
			var personsThatGotABadge = new List<IPerson>();
			if (agentsThatShouldGetBadge != null)
			{
				foreach (
					var person in
						agentsThatShouldGetBadge.Select(agent => allPersons.Single(x => x.Id != null && x.Id.Value == agent))
							.Where(a => a != null))
				{
					var badge = person.Badges.First(x => x.BadgeType == badgeType);
					if (badge == null || badge.LastCalculatedDate < date)
					{
						person.AddBadge(new Domain.Common.AgentBadge
						{
							BronzeBadge = 1,
							BadgeType = badgeType,
							LastCalculatedDate = date
						});
						personsThatGotABadge.Add(person);
					}
				}
			}

			return personsThatGotABadge;
		}

		public IEnumerable<IPerson> Calculate(IEnumerable<IPerson> allPersons, string timezoneCode,
			DateOnly date, AdherenceReportSettingCalculationMethod adherenceCalculationMethod, int silverToBronzeBadgeRate, int goldToSilverBadgeRate)
		{
			var personsThatGotBadge = new List<IPerson>();
			var agents = _statisticRepository.LoadAgentsOverThresholdForAdherence(adherenceCalculationMethod, timezoneCode, date);
			if (agents != null)
			{
				var personsThatGotAAdherenceBadge = AddBadge(allPersons, agents, BadgeType.Adherence, silverToBronzeBadgeRate, goldToSilverBadgeRate, date);
				personsThatGotBadge.AddRange(personsThatGotAAdherenceBadge);
			}

			agents = _statisticRepository.LoadAgentsOverThresholdForAnsweredCalls(timezoneCode, date);
			if (agents != null)
			{
				var personsThatGotAAnsweredCallsBadge = AddBadge(allPersons, agents, BadgeType.AnsweredCalls, silverToBronzeBadgeRate, goldToSilverBadgeRate, date);
				personsThatGotBadge.AddRange(personsThatGotAAnsweredCallsBadge);
			}

			agents = _statisticRepository.LoadAgentsUnderThresholdForAHT(timezoneCode, date);
			if (agents != null)
			{
				var personsThatGotAAHTBadge = AddBadge(allPersons, agents, BadgeType.AverageHandlingTime, silverToBronzeBadgeRate, goldToSilverBadgeRate, date);
				personsThatGotBadge.AddRange(personsThatGotAAHTBadge);
			}

			return personsThatGotBadge;
		}
	}
}
