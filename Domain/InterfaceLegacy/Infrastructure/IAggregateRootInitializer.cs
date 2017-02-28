using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IAggregateRootInitializer
	{
		void Initialize(IEnumerable<IPersonAssignment> pas);
		void Initialize(IEnumerable<IPersonAbsence> pas);
	}
}
