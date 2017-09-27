using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleAuditTrailReport : IScheduleAuditTrailReport
	{
		private readonly IUserTimeZone _timeZone;
		private readonly IList<IPerson> modifiedByList = new List<IPerson>();
		private readonly IList<ScheduleAuditingReportData> auditingReportList = new List<ScheduleAuditingReportData>();

		public FakeScheduleAuditTrailReport(IUserTimeZone timeZone)
		{
			_timeZone = timeZone;
		}

		public void AddModifiedByPerson(IPerson personThatModified)
		{
			modifiedByList.Add(personThatModified);
		}
		
		public IEnumerable<IPerson> RevisionPeople()
		{
			return modifiedByList;
		}

		public IList<ScheduleAuditingReportData> Report(IPerson changedByPerson, DateOnlyPeriod changedPeriod, DateOnlyPeriod scheduledPeriod)
		{

			var hits = auditingReportList
				.Where(x => x.ModifiedBy == changedByPerson.Id.Value.ToString() 
							&& changedPeriod.ToDateTimePeriod(_timeZone.TimeZone()).Contains(x.ModifiedAt) 
							&& scheduledPeriod.ToDateTimePeriod(_timeZone.TimeZone()).Contains(x.ScheduleStart))
				.ToList();

			foreach (var auditItem in hits)
			{
				auditItem.ModifiedAt = TimeZoneHelper.ConvertFromUtc(auditItem.ModifiedAt, _timeZone.TimeZone());
				auditItem.ScheduleStart = TimeZoneHelper.ConvertFromUtc(auditItem.ScheduleStart, _timeZone.TimeZone());
				auditItem.ScheduleEnd = TimeZoneHelper.ConvertFromUtc(auditItem.ScheduleEnd, _timeZone.TimeZone());
			}

			return hits;
		}

		public void Has(ScheduleAuditingReportData scheduleAuditingReportData)
		{
			auditingReportList.Add(scheduleAuditingReportData);
		}
	}
}
