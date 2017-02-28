using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IAggregateRootInitializer
	{
		void Initialize(IEnumerable<IPersonAssignment> pas);
		void Initialize(IEnumerable<IPersonAbsence> pas);
	}
}
