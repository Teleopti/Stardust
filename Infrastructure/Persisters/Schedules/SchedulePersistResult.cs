using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class SchedulePersistResult
	{
		public IInitiatorIdentifier InitiatorIdentifier { get; set; }
		public IEnumerable<PersistConflict> PersistConflicts { get; set; } 
		public IEnumerable<IRootChangeInfo> ModifiedRoots { get; set; }

		public SchedulePersistResult()
		{
			PersistConflicts = Enumerable.Empty<PersistConflict>();
			ModifiedRoots = Enumerable.Empty<IRootChangeInfo>();
		}

	}
}