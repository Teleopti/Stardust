using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Envers;
using NHibernate.Envers.Query;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Infrastructure.Repositories.Audit
{
	public class ScheduleHistoryReport : IScheduleHistoryReport
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRegional _regional;

		public ScheduleHistoryReport(IUnitOfWorkFactory unitOfWorkFactory, IRegional regional)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_regional = regional;
		}

		public IEnumerable<ScheduleAuditingReportData> Report(IPerson modifiedBy, DateOnlyPeriod changedPeriod, DateOnlyPeriod scheduledPeriod, IEnumerable<IPerson> agents, int maximumRows)
		{			
			var auditSession = session().Auditer();
			var ret = new List<ScheduleAuditingReportData>();
			var changedPeriodAgentTimeZone = changedPeriod.ToDateTimePeriod(_regional.TimeZone);
			var scheduledPeriodAgentTimeZone = scheduledPeriod.ToDateTimePeriod(_regional.TimeZone);

			foreach (var agentsBatch in agents.Batch(100))
			{
				var retTemp = new List<ScheduleAuditingReportData>();

				auditSession.CreateQuery().ForHistoryOf<PersonAssignment, Revision>()
				.Add(AuditEntity.RevisionProperty("ModifiedAt").Between(changedPeriodAgentTimeZone.StartDateTime, changedPeriodAgentTimeZone.EndDateTime))
				.AddModifiedByIfNotNull(modifiedBy)
				.Add(AuditEntity.Property("Date").Between(scheduledPeriod.StartDate, scheduledPeriod.EndDate))
				.Add(AuditEntity.Property("Person").In(agentsBatch))
				.Results()
				.ForEach(assRev => retTemp.Add(createAssignmentAuditingData(assRev)));

				auditSession.CreateQuery().ForHistoryOf<PersonAbsence, Revision>()
					.Add(AuditEntity.RevisionProperty("ModifiedAt").Between(changedPeriodAgentTimeZone.StartDateTime, changedPeriodAgentTimeZone.EndDateTime))
					.AddModifiedByIfNotNull(modifiedBy)
					.Add(AuditEntity.Property("Layer.Period.period.Minimum").Lt(scheduledPeriodAgentTimeZone.EndDateTime))
					.Add(AuditEntity.Property("Layer.Period.period.Maximum").Gt(scheduledPeriodAgentTimeZone.StartDateTime))
					.Add(AuditEntity.Property("Person").In(agentsBatch))
					.Results()
					.ForEach(absRev => retTemp.Add(createAbsenceAuditingData(absRev)));
				ret.AddRange(retTemp);
				if (ret.Count > maximumRows)
					break;
			}
			return ret;
		}

		public IEnumerable<ScheduleAuditingReportData> Report(DateOnlyPeriod changedPeriod, DateOnlyPeriod scheduledPeriod, IEnumerable<IPerson> agents, int maximumRows)
		{
			return Report(null, changedPeriod, scheduledPeriod, agents, maximumRows);
		}

		// WARNING: This query caused extra query to load PersonWriteProtection, it may cause performance problem for large customer.
		// Refer to bug #47923: Loading Business hierarchy and changed by in Schedule Audit report for large installations is slow
		public IEnumerable<IPerson> RevisionPeople()
		{
			return session().GetNamedQuery("RevisionPeople").List<IPerson>();
		}

		private ScheduleAuditingReportData createAssignmentAuditingData(IRevisionEntityInfo<PersonAssignment, Revision> auditedAssignment)
		{
			var ret = new ScheduleAuditingReportData { ShiftType = Resources.AuditingReportShift};
			addCommonScheduleData(ret, auditedAssignment.Entity, auditedAssignment.RevisionEntity, auditedAssignment.Operation);


			ret.Detail = string.Empty;
			var personAssignment = auditedAssignment.Entity;

			if(personAssignment.ShiftCategory != null)
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

			ret.ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(period.StartDateTime, _regional.TimeZone);
			ret.ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(period.EndDateTime, _regional.TimeZone);

			return ret;
		}

		private ScheduleAuditingReportData createAbsenceAuditingData(IRevisionEntityInfo<PersonAbsence, Revision> auditedAbsence)
		{
			var ret = new ScheduleAuditingReportData
			{
				ShiftType = Resources.AuditingReportAbsence,
				Detail = auditedAbsence.Entity.Layer.Payload.Description.Name,
				ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(auditedAbsence.Entity.Period.StartDateTime, _regional.TimeZone),
                ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(auditedAbsence.Entity.Period.EndDateTime, _regional.TimeZone)
			};
			addCommonScheduleData(ret, auditedAbsence.Entity, auditedAbsence.RevisionEntity, auditedAbsence.Operation);
			return ret;
		}

		private void addCommonScheduleData(ScheduleAuditingReportData scheduleAuditingReportData, 
																IPersistableScheduleData auditedEntity, 
																Revision revision,
																RevisionType revisionType)
		{
			scheduleAuditingReportData.ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(revision.ModifiedAt, _regional.TimeZone);
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

		private ISession session()
		{
			return ((NHibernateUnitOfWork)_unitOfWorkFactory.CurrentUnitOfWork()).Session;
		}
	}
}