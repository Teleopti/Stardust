using System.Collections.Generic;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Reports;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IScheduleAuditTrailReport
	{
		IEnumerable<SimplestPersonInfo> GetRevisionPeople();
		IList<ScheduleAuditingReportData> Report(IPerson changedByPerson, DateOnlyPeriod changedPeriod,
			DateOnlyPeriod scheduledPeriod, int maximumResults, IList<IPerson> scheduledAgents);
	}
}