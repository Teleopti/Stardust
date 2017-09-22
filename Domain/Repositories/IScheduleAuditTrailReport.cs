using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IScheduleAuditTrailReport
	{
		IEnumerable<IPerson> RevisionPeople();
	}
}