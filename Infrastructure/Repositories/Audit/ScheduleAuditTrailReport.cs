using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Envers;
using NHibernate.Envers.Query;
//using NHibernate.Envers;
//using NHibernate.Envers.Query;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories.Audit
{
	public class ScheduleAuditTrailReport : IScheduleAuditTrailReport
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IUserTimeZone _timeZone;

		public ScheduleAuditTrailReport(ICurrentUnitOfWork currentUnitOfWork, IUserTimeZone timeZone)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_timeZone = timeZone;
		}
		public IEnumerable<IPerson> RevisionPeople()
		{
			return _currentUnitOfWork.Current().Session().GetNamedQuery("RevisionPeople").List<IPerson>();
		}

		public IList<ScheduleAuditingReportData> Report(IPerson changedByPerson, DateOnlyPeriod changedPeriod, DateOnlyPeriod scheduledPeriod, int maximumResults)
		{
			var auditSession = _currentUnitOfWork.Current().Session().Auditer();
			var ret = new List<ScheduleAuditingReportData>();
			var changedPeriodAgentTimeZone = changedPeriod.ToDateTimePeriod(_timeZone.TimeZone());
			var scheduledPeriodAgentTimeZone = scheduledPeriod.ToDateTimePeriod(_timeZone.TimeZone());

			var retTemp = new List<ScheduleAuditingReportData>();
			if (maximumResults == 0)
				maximumResults = 50000;

			auditSession.CreateQuery().ForHistoryOf<PersonAssignment, Revision>()
				.Add(AuditEntity.RevisionProperty("ModifiedAt").Between(changedPeriodAgentTimeZone.StartDateTime, changedPeriodAgentTimeZone.EndDateTime))
				.AddModifiedByIfNotNull(changedByPerson)
				.Add(AuditEntity.Property("Date").Between(scheduledPeriod.StartDate, scheduledPeriod.EndDate))
				.AddOrder(AuditEntity.RevisionProperty("ModifiedAt").Desc())
				.SetMaxResults(maximumResults)
				.Results()
				.ForEach(assRev => retTemp.Add(createAssignmentAuditingData(assRev)));

			auditSession.CreateQuery().ForHistoryOf<PersonAbsence, Revision>()
				.Add(AuditEntity.RevisionProperty("ModifiedAt").Between(changedPeriodAgentTimeZone.StartDateTime, changedPeriodAgentTimeZone.EndDateTime))
				.AddModifiedByIfNotNull(changedByPerson)
				.Add(AuditEntity.Property("Layer.Period.period.Minimum").Lt(scheduledPeriodAgentTimeZone.EndDateTime))
				.Add(AuditEntity.Property("Layer.Period.period.Maximum").Gt(scheduledPeriodAgentTimeZone.StartDateTime))
				.AddOrder(AuditEntity.RevisionProperty("ModifiedAt").Desc())
				.SetMaxResults(maximumResults)
				.Results()
				.ForEach(absRev => retTemp.Add(createAbsenceAuditingData(absRev)));

			ret.AddRange(retTemp
				.OrderByDescending(o => o.ModifiedAt)
				.Take(maximumResults));
			
			return ret;
		}

		private ScheduleAuditingReportData createAssignmentAuditingData(IRevisionEntityInfo<PersonAssignment, Revision> auditedAssignment)
		{
			var ret = new ScheduleAuditingReportData { ShiftType = Resources.AuditingReportShift };
			addCommonScheduleData(ret, auditedAssignment.Entity, auditedAssignment.RevisionEntity, auditedAssignment.Operation);


			ret.Detail = string.Empty;
			var personAssignment = auditedAssignment.Entity;

			if (personAssignment.ShiftCategory != null)
				ret.Detail = personAssignment.ShiftCategory.Description.Name;

			if (personAssignment.DayOff() != null)
				ret.Detail = personAssignment.DayOff().Description.Name;

			if (personAssignment.ShiftCategory == null && personAssignment.DayOff() == null)
			{
				if (personAssignment.OvertimeActivities().IsEmpty() && !personAssignment.PersonalActivities().IsEmpty())
					ret.Detail = Resources.PersonalShift;

				if (personAssignment.PersonalActivities().IsEmpty() && !personAssignment.OvertimeActivities().IsEmpty())
					ret.Detail = Resources.Overtime;

			}

			var period = auditedAssignment.Entity.Period;

			ret.ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(period.StartDateTime, _timeZone.TimeZone());
			ret.ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(period.EndDateTime, _timeZone.TimeZone());

			return ret;
		}

		private ScheduleAuditingReportData createAbsenceAuditingData(IRevisionEntityInfo<PersonAbsence, Revision> auditedAbsence)
		{
			var ret = new ScheduleAuditingReportData
			{
				ShiftType = Resources.AuditingReportAbsence,
				Detail = auditedAbsence.Entity.Layer.Payload.Description.Name,
				ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(auditedAbsence.Entity.Period.StartDateTime, _timeZone.TimeZone()),
				ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(auditedAbsence.Entity.Period.EndDateTime, _timeZone.TimeZone())
			};
			addCommonScheduleData(ret, auditedAbsence.Entity, auditedAbsence.RevisionEntity, auditedAbsence.Operation);
			return ret;
		}

		private void addCommonScheduleData(ScheduleAuditingReportData scheduleAuditingReportData,
			IPersistableScheduleData auditedEntity,
			Revision revision,
			RevisionType revisionType)
		{
			scheduleAuditingReportData.ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(revision.ModifiedAt, _timeZone.TimeZone());
			scheduleAuditingReportData.ModifiedBy = revision.ModifiedBy.Name.ToString(NameOrderOption.FirstNameLastName);
			scheduleAuditingReportData.ScheduledAgent = auditedEntity.Person.Name.ToString(NameOrderOption.FirstNameLastName);
			addRevisionType(scheduleAuditingReportData, revisionType);
		}

		private static void addRevisionType(ScheduleAuditingReportData scheduleAuditingReportData, RevisionType revisionType)
		{
			switch (revisionType)
			{
				case RevisionType.Added:
					scheduleAuditingReportData.AuditType = Resources.AuditingReportInsert;
					break;
				case RevisionType.Deleted:
					scheduleAuditingReportData.AuditType = Resources.AuditingReportDeleted;
					break;
				case RevisionType.Modified:
					scheduleAuditingReportData.AuditType = Resources.AuditingReportModified;
					break;
			}
		}
	}
}
