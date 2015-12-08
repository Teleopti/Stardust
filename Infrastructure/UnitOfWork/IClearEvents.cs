using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public interface IClearEvents
	{
		void Execute(IEnumerable<IAggregateRoot> aggregateRoots);
	}
}