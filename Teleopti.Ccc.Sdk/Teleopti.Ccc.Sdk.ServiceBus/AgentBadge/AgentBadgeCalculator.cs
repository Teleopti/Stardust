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

		protected static IList<IPerson> addBadge(IEnumerable<IPerson> allPersons, IEnumerable<Guid> agentsThatShouldGetBadge)
		{
			var personsThatGotABadge = new List<IPerson>();
			if (agentsThatShouldGetBadge != null)
			{
				foreach (
					var person in
						agentsThatShouldGetBadge.Select(agent => allPersons.Single(x => x.Id.Value == agent)).Where(a => a != null))
				{
					person.AddBadge(new Domain.Common.AgentBadge { BronzeBadge = 1 });
					personsThatGotABadge.Add(person);
				}
			}
			return personsThatGotABadge;
		}

		public IEnumerable<IPerson> Calculate(IUnitOfWork unitOfWork, IEnumerable<IPerson> allPersons, Tuple<int, string, int> timeZone, AdherenceReportSettingCalculationMethod adherenceCalculationMethod)
		{
			var agentsThatShouldGetBadge = new List<Guid>();
			var date = DateTime.UtcNow.AddMinutes(timeZone.Item3).Date.AddDays(-1);
			var agents = _statisticRepository.LoadAgentsOverThresholdForAdherence(unitOfWork, adherenceCalculationMethod,date);
			if (agents != null)
			{
				agentsThatShouldGetBadge.AddRange(agents);
			}

			agents = _statisticRepository.LoadAgentsOverThresholdForAnsweredCalls(unitOfWork, date);
			if (agents != null)
			{
				agentsThatShouldGetBadge.AddRange(agents);
			}

			agents = _statisticRepository.LoadAgentsUnderThresholdForAHT(unitOfWork, date);
			if (agents != null)
			{
				agentsThatShouldGetBadge.AddRange(agents);
			}

			var personsThatGotABadge = addBadge(allPersons, agentsThatShouldGetBadge);
			return personsThatGotABadge;
		}
	}
}
