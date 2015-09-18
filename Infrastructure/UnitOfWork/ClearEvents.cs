using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class ClearEvents : IClearEvents
	{
		public void Execute(IEnumerable<IAggregateRoot> aggregateRoots)
		{
			aggregateRoots.OfType<IAggregateRootWithEvents>().ForEach(a => a.PopAllEvents());
		}
	}
}