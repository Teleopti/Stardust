using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class AggregateRootInitilizer : IAggregateRootInitializer
	{
		public void Initialize(IEnumerable<IPersonAssignment> personAssignments)
		{
			var initializerPersonAssignment = new InitializeRootsPersonAssignment(personAssignments);
			initializerPersonAssignment.Initialize();
		}

		public void Initialize(IEnumerable<IPersonAbsence> personAbsences)
		{
			var initializerPersonAbsence = new InitializeRootsPersonAbsence(personAbsences);
			initializerPersonAbsence.Initialize();
		}
	}
}
