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

		public AgentBadgeCalculator(IStatisticRepository statisticRepository)
		{
			_statisticRepository = statisticRepository;
		}

		protected static IList<IPerson> addBadge(IEnumerable<IPerson> allPersons, IEnumerable<Guid> agentsThatShouldGetBadge, BadgeType badgeType)
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
