using System.Collections.Generic;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IScheduleHistoryReport
	{
		IEnumerable<ScheduleAuditingReportData> Report(IPerson modifiedBy, 
		                                               DateOnlyPeriod changedPeriod, 
		                                               DateOnlyPeriod scheduledPeriod, 
		                                               IEnumerable<IPerson> agents,
														int maximumRows);

		IEnumerable<ScheduleAuditingReportData> Report(DateOnlyPeriod changedPeriod, 
																		DateOnlyPeriod scheduledPeriod, 
																		IEnumerable<IPerson> agents,
																		int maximumRows);

		IEnumerable<IPerson> RevisionPeople();
	}
}