using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class AgentBadgeAnsweredCallsCalculator : AgentBadgeCalculator, IAgentBadgeCalculator
	{
		private readonly IStatisticRepository _statisticRepository;

		public AgentBadgeAnsweredCallsCalculator(IStatisticRepository statisticRepository)
		{
			_statisticRepository = statisticRepository;
		}

		public IEnumerable<IPerson> Calculate(IUnitOfWork unitOfWork, IEnumerable<IPerson> allPersons)
		{
			var agentsThatShouldGetBadge = _statisticRepository.LoadAgentsOverThresholdForAnsweredCalls(unitOfWork);

			var personsThatGotABadge = addBadge(allPersons, agentsThatShouldGetBadge);
			return personsThatGotABadge;
		}
	}
}