using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class AgentBadgeCalculator : IAgentBadgeCalculator
	{
		private readonly IStatisticRepository _statisticRepository;
		private const int silverBadgeRate = 5;
		private const int goldBadgeRate = 10;
		private const int goldToSilverBadgeRate = goldBadgeRate/silverBadgeRate;

		public AgentBadgeCalculator(IStatisticRepository statisticRepository)
		{
			_statisticRepository = statisticRepository;
		}

		protected IList<IPerson> addBadge(IEnumerable<IPerson> allPersons, IEnumerable<Guid> agentsThatShouldGetBadge, BadgeType badgeType)
		{
			var personsThatGotABadge = new List<IPerson>();
			if (agentsThatShouldGetBadge != null)
			{
				foreach (
					var person in
						agentsThatShouldGetBadge.Select(agent => allPersons.Single(x => x.Id.Value == agent)).Where(a => a != null))
				{
					person.AddBadge(new Domain.Common.AgentBadge { BronzeBadge = 1, BadgeType = badgeType});
					personsThatGotABadge.Add(person);
				}
			}

			foreach (var person in personsThatGotABadge)
			{
				if (person.Badges.Any(x => x.BadgeType == BadgeType.AnsweredCalls))
				{
					var answeredCallsBadge = person.Badges.Single(x => x.BadgeType == BadgeType.AnsweredCalls);
					if (answeredCallsBadge.BronzeBadge >= silverBadgeRate)
					{
						answeredCallsBadge.SilverBadge = answeredCallsBadge.SilverBadge + answeredCallsBadge.BronzeBadge / silverBadgeRate;
						answeredCallsBadge.BronzeBadge = answeredCallsBadge.BronzeBadge%silverBadgeRate;
					}
					if (answeredCallsBadge.SilverBadge >= goldToSilverBadgeRate)
					{
						answeredCallsBadge.GoldBadge = answeredCallsBadge.GoldBadge + answeredCallsBadge.SilverBadge / goldToSilverBadgeRate;
						answeredCallsBadge.SilverBadge = answeredCallsBadge.SilverBadge % goldToSilverBadgeRate;
					}
				}
				if (person.Badges.Any(x => x.BadgeType == BadgeType.AverageHandlingTime))
				{
					var averageHandlingTimeBadge = person.Badges.Single(x => x.BadgeType == BadgeType.AverageHandlingTime);
					if (averageHandlingTimeBadge.BronzeBadge >= silverBadgeRate)
					{
						averageHandlingTimeBadge.SilverBadge = averageHandlingTimeBadge.SilverBadge + averageHandlingTimeBadge.BronzeBadge / silverBadgeRate;
						averageHandlingTimeBadge.BronzeBadge = averageHandlingTimeBadge.BronzeBadge % silverBadgeRate;
					}
					if (averageHandlingTimeBadge.SilverBadge > goldToSilverBadgeRate)
					{
						averageHandlingTimeBadge.GoldBadge = averageHandlingTimeBadge.GoldBadge + averageHandlingTimeBadge.SilverBadge / goldToSilverBadgeRate;
						averageHandlingTimeBadge.SilverBadge = averageHandlingTimeBadge.SilverBadge % goldToSilverBadgeRate;
					}
				}
				if (person.Badges.Any(x => x.BadgeType == BadgeType.Adherence))
				{
					var adherenceBadge = person.Badges.Single(x => x.BadgeType == BadgeType.Adherence);
					if (adherenceBadge.BronzeBadge > silverBadgeRate)
					{
						adherenceBadge.SilverBadge = adherenceBadge.SilverBadge + adherenceBadge.BronzeBadge / silverBadgeRate;
						adherenceBadge.BronzeBadge = adherenceBadge.BronzeBadge % silverBadgeRate;
					}
					if (adherenceBadge.SilverBadge > goldToSilverBadgeRate)
					{
						adherenceBadge.GoldBadge = adherenceBadge.GoldBadge + adherenceBadge.SilverBadge / goldToSilverBadgeRate;
						adherenceBadge.SilverBadge = adherenceBadge.SilverBadge % goldToSilverBadgeRate;
					}
				}
			}
			return personsThatGotABadge;
		}

		public IEnumerable<IPerson> Calculate(IUnitOfWork unitOfWork, IEnumerable<IPerson> allPersons, int timezoneId, DateTime date, AdherenceReportSettingCalculationMethod adherenceCalculationMethod)
		{
			var personsThatGotBadge = new List<IPerson>();
			var agents = _statisticRepository.LoadAgentsOverThresholdForAdherence(unitOfWork, adherenceCalculationMethod, timezoneId, date);
			if (agents != null)
			{
				var personsThatGotAAdherenceBadge = addBadge(allPersons, agents, BadgeType.Adherence);
				personsThatGotBadge.AddRange(personsThatGotAAdherenceBadge);
			}

			agents = _statisticRepository.LoadAgentsOverThresholdForAnsweredCalls(unitOfWork, timezoneId, date);
			if (agents != null)
			{
				var personsThatGotAAnsweredCallsBadge = addBadge(allPersons, agents, BadgeType.AnsweredCalls);
				personsThatGotBadge.AddRange(personsThatGotAAnsweredCallsBadge);
			}

			agents = _statisticRepository.LoadAgentsUnderThresholdForAHT(unitOfWork, timezoneId, date);
			if (agents != null)
			{
				var personsThatGotAAHTBadge = addBadge(allPersons, agents, BadgeType.AverageHandlingTime);
				personsThatGotBadge.AddRange(personsThatGotAAHTBadge);
			}

			return personsThatGotBadge;
		}

		public IDictionary<int, DateTime> LastCalculatedDates { get; set; }
	}
}
