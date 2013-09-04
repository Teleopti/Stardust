using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters
{
	public class NoReassociateData : IReassociateData
	{
		public IEnumerable<IAggregateRoot>[] DataToReassociate(IPerson personToReassociate) { return new[] {new IAggregateRoot[] {}}; }
	}
}