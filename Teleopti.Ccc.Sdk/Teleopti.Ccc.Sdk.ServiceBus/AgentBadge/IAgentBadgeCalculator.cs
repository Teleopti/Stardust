using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public interface IAgentBadgeCalculator
	{
		IEnumerable<IPerson> Calculate(IUnitOfWork unitOfWork, IEnumerable<IPerson> allPersons);
	}
}