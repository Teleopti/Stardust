using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters.Requests
{
	public interface IRequestPersister
	{
		void Persist(IEnumerable<IPersonRequest> requests);
	}
}