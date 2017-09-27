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
		IList<ScheduleAuditingReportData> Report(IPerson changedByPerson, DateOnlyPeriod changedPeriod, DateOnlyPeriod scheduledPeriod, int maximumResults);
	}
}