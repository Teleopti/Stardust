using System.Collections.Generic;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleAuditTrailReport : IScheduleAuditTrailReport
	{
		private readonly IList<IPerson> modifiedByList = new List<IPerson>();
		private readonly IList<ScheduleAuditingReportData> auditingReportList = new List<ScheduleAuditingReportData>();

		public void AddModifiedByPerson(IPerson PersonThatModified)
		{
			modifiedByList.Add(PersonThatModified);
		}
		
		public IEnumerable<IPerson> RevisionPeople()
		{
			return modifiedByList;
		}

		public void Has(ScheduleAuditingReportData scheduleAuditingReportData)
		{
			auditingReportList.Add(scheduleAuditingReportData);
		}
	}
}
