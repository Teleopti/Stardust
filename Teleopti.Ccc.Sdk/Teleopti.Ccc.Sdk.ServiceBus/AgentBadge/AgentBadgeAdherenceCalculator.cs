using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class AgentBadgeAdherenceCalculator : AgentBadgeCalculator, IAgentBadgeCalculator
	{
		private readonly IStatisticRepository _statisticRepository;

		public AgentBadgeAdherenceCalculator(IStatisticRepository statisticRepository)
		{
			_statisticRepository = statisticRepository;
		}

		public IEnumerable<IPerson> Calculate(IUnitOfWork unitOfWork, IEnumerable<IPerson> allPersons)
		{
			var agentsThatShouldGetBadge = _statisticRepository.LoadAgentsOverThresholdForAdherence(unitOfWork);

			var personsThatGotABadge = addBadge(allPersons, agentsThatShouldGetBadge);
			return personsThatGotABadge;
		}
	}

	public class AgentBadgeAHTCalculator : AgentBadgeCalculator, IAgentBadgeCalculator
	{
		private readonly IStatisticRepository _statisticRepository;

		public AgentBadgeAHTCalculator(IStatisticRepository statisticRepository)
		{
			_statisticRepository = statisticRepository;
		}

		public IEnumerable<IPerson> Calculate(IUnitOfWork unitOfWork, IEnumerable<IPerson> allPersons)
		{
			var agentsThatShouldGetBadge = _statisticRepository.LoadAgentsUnderThresholdForAHT(unitOfWork);

			var personsThatGotABadge = addBadge(allPersons, agentsThatShouldGetBadge);
			return personsThatGotABadge;
		}
	}
}