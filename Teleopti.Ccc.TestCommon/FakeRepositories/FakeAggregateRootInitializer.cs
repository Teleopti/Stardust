using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAggregateRootInitializer : IAggregateRootInitializer
	{
		public void Initialize(IEnumerable<IPersonAssignment> pas)
		{
			// do nothing
		}

		public void Initialize(IEnumerable<IPersonAbsence> pas)
		{
			// do nothing
		}
	}
}
