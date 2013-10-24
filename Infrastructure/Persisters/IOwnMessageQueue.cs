using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IOwnMessageQueue
	{
		void ReassociateDataWithAllPeople();
		IEnumerable<IAggregateRoot>[] DataToReassociate(IPerson personToReassociate);
	}
}