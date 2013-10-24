using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IReassociateDataForSchedules
	{
		void ReassociateDataForAllPeople();
		IEnumerable<IAggregateRoot>[] DataToReassociate(IPerson personToReassociate);
	}
}