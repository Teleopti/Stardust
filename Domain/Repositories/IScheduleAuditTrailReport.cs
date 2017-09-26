using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IScheduleAuditTrailReport
	{
		IEnumerable<IPerson> RevisionPeople();
		IList<ScheduleAuditingReportData> Report(Guid changedByPersonId, DateTimePeriod changeOccurredPeriod, DateTimePeriod affectedPeriod);
	}
}