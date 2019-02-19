using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Domain.Repositories;


namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeScheduleAuditTrailReport : IScheduleAuditTrailReport
	{
		private readonly IUserTimeZone _timeZone;
		private readonly IList<auditedBy> modifiedByList = new List<auditedBy>();

		private readonly IList<ScheduleAuditingReportDataForTest> auditingReportList =
			new List<ScheduleAuditingReportDataForTest>();

		public FakeScheduleAuditTrailReport(IUserTimeZone timeZone)
		{
			_timeZone = timeZone;
		}

		public void AddModifiedByPerson(IPerson personThatModified, DateOnly modifiedAt)
		{
			modifiedByList.Add(new auditedBy(){person = personThatModified, modifiedAt = modifiedAt});
		}

		public IEnumerable<SimplestPersonInfo> GetRevisionPeople(DateOnlyPeriod searchPeriod)
		{
			return modifiedByList
				.Where(y => searchPeriod.Contains(y.modifiedAt))
				.Select(x => new SimplestPersonInfo
				{
					Id = x.person.Id.GetValueOrDefault(),
					Name = x.person.Name.ToString()
				});
		}

		public IList<ScheduleAuditingReportData> Report(IPerson changedByPerson, DateOnlyPeriod changedPeriod,
			DateOnlyPeriod scheduledPeriod, int maximumResults, IList<IPerson> scheduledAgents)
		{
			IList<ScheduleAuditingReportDataForTest> hits;

			if (changedByPerson == null)
			{
				hits = auditingReportList
					.Where(x => changedPeriod.ToDateTimePeriod(_timeZone.TimeZone()).Contains(x.ModifiedAt)
								&& scheduledPeriod.ToDateTimePeriod(_timeZone.TimeZone()).Contains(x.ScheduleStart)
								&& scheduledAgents.Select(y => y.Id.Value).Contains(x.scheduleAgentId))
					.Take(maximumResults)
					.ToList();
			}
			else
			{
				hits = auditingReportList
					.Where(x => x.ModifiedBy == changedByPerson.Id.Value.ToString()
								&& changedPeriod.ToDateTimePeriod(_timeZone.TimeZone()).Contains(x.ModifiedAt)
								&& scheduledPeriod.ToDateTimePeriod(_timeZone.TimeZone()).Contains(x.ScheduleStart)
								&& scheduledAgents.Select(y => y.Id.Value).Contains(x.scheduleAgentId))
					.Take(maximumResults)
					.ToList();
			}

			foreach (var auditItem in hits)
			{
				auditItem.ModifiedAt = TimeZoneHelper.ConvertFromUtc(auditItem.ModifiedAt, _timeZone.TimeZone());
				auditItem.ScheduleStart = TimeZoneHelper.ConvertFromUtc(auditItem.ScheduleStart, _timeZone.TimeZone());
				auditItem.ScheduleEnd = TimeZoneHelper.ConvertFromUtc(auditItem.ScheduleEnd, _timeZone.TimeZone());
			}

			var ret = hits.Select(y => y as ScheduleAuditingReportData).ToList();
			return ret;
		}

		public void Has(ScheduleAuditingReportDataForTest scheduleAuditingReportData)
		{
			auditingReportList.Add(scheduleAuditingReportData);
		}
	}

	public class ScheduleAuditingReportDataForTest : ScheduleAuditingReportData
	{
		public Guid scheduleAgentId { get; set; }
	}

	public class auditedBy
	{
		public IPerson person { get; set; }
		public DateOnly modifiedAt { get; set; }
	}
}
