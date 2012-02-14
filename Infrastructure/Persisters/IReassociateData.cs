using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public interface IReassociateData
	{
		IEnumerable<IAggregateRoot>[] DataToReassociate(IPerson personToReassociate);
	}
}