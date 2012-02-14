using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Envers;
using NHibernate.Envers.Query;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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

		public IEnumerable<ScheduleAuditingReportData> Report(IPerson modifiedBy, DateOnlyPeriod changedPeriod, DateOnlyPeriod scheduledPeriod, IEnumerable<IPerson> agents)
		{			
			var auditSession = session().Auditer();
			var ret = new List<ScheduleAuditingReportData>();

			auditSession.CreateQuery().ForHistoryOf<PersonAssignment, Revision>()
				.Add(AuditEntity.RevisionProperty("ModifiedAt").Between(changedPeriod.StartDate.Date, changedPeriod.EndDate.Date))
				.AddModifiedByIfNotNull(modifiedBy)
				.Add(AuditEntity.Property("Period.period.Minimum").Le(scheduledPeriod.EndDate.Date))
				.Add(AuditEntity.Property("Period.period.Maximum").Ge(scheduledPeriod.StartDate.Date))
				.Add(AuditEntity.Property("Person").In(agents))
				.Results()
				.ForEach(assRev => ret.Add(createAssignmentAuditingData(assRev)));

			auditSession.CreateQuery().ForHistoryOf<PersonDayOff, Revision>()
				.Add(AuditEntity.RevisionProperty("ModifiedAt").Between(changedPeriod.StartDate.Date, changedPeriod.EndDate.Date))
				.AddModifiedByIfNotNull(modifiedBy)
				.Add(AuditEntity.Property("DayOff.Anchor").Between(scheduledPeriod.StartDate.Date, scheduledPeriod.EndDate.Date))
				.Add(AuditEntity.Property("Person").In(agents))
				.Results()
				.ForEach(dayOffRev => ret.Add(createDayOffAuditingData(dayOffRev)));

			auditSession.CreateQuery().ForHistoryOf<PersonAbsence, Revision>()
				.Add(AuditEntity.RevisionProperty("ModifiedAt").Between(changedPeriod.StartDate.Date, changedPeriod.EndDate.Date))
				.AddModifiedByIfNotNull(modifiedBy)
				.Add(AuditEntity.Property("Layer.Period.period.Minimum").Le(scheduledPeriod.EndDate.Date))
				.Add(AuditEntity.Property("Layer.Period.period.Maximum").Ge(scheduledPeriod.StartDate.Date))
				.Add(AuditEntity.Property("Person").In(agents))
				.Results()
				.ForEach(absRev => ret.Add(createAbsenceAuditingData(absRev)));

			return ret;
		}

		public IEnumerable<ScheduleAuditingReportData> Report(DateOnlyPeriod changedPeriod, DateOnlyPeriod scheduledPeriod, IEnumerable<IPerson> agents)
		{
			return Report(null, changedPeriod, scheduledPeriod, agents);
		}

		public IEnumerable<IPerson> RevisionPeople()
		{
			return session().GetNamedQuery("RevisionPeople").List<IPerson>();
		}

		private ScheduleAuditingReportData createAssignmentAuditingData(IRevisionEntityInfo<PersonAssignment, Revision> auditedAssignment)
		{
			var ret = new ScheduleAuditingReportData { ShiftType = Resources.AuditingReportShift};
			addCommonScheduleData(ret, auditedAssignment.Entity, auditedAssignment.RevisionEntity, auditedAssignment.Operation);

			if (auditedAssignment.Entity.MainShift == null)
			{
				ret.Detail = string.Empty;
			}
			else
			{
				ret.Detail = auditedAssignment.Entity.MainShift.ShiftCategory.Description.Name;
			}

			if (auditedAssignment.Entity.DatabasePeriod.Equals(PersonAssignment.UndefinedPeriod))
			{
				ret.ScheduleStart = DateTime.MinValue;
				ret.ScheduleEnd = DateTime.MinValue;
			}
			else
			{
				ret.ScheduleStart = _regional.TimeZone.ConvertTimeFromUtc(auditedAssignment.Entity.DatabasePeriod.StartDateTime);
				ret.ScheduleEnd = _regional.TimeZone.ConvertTimeFromUtc(auditedAssignment.Entity.DatabasePeriod.EndDateTime);
			}

			return ret;
		}

		private ScheduleAuditingReportData createDayOffAuditingData(IRevisionEntityInfo<PersonDayOff, Revision> auditedDayOff)
		{
			var ret = new ScheduleAuditingReportData
			{
				ShiftType = Resources.AuditingReportDayOff,
				Detail = auditedDayOff.Entity.DayOff.Description.Name,
				ScheduleStart = _regional.TimeZone.ConvertTimeFromUtc(auditedDayOff.Entity.Period.StartDateTime),
				ScheduleEnd = _regional.TimeZone.ConvertTimeFromUtc(auditedDayOff.Entity.Period.EndDateTime)
			};
			addCommonScheduleData(ret, auditedDayOff.Entity, auditedDayOff.RevisionEntity, auditedDayOff.Operation);
			return ret;
		}

		private ScheduleAuditingReportData createAbsenceAuditingData(IRevisionEntityInfo<PersonAbsence, Revision> auditedAbsence)
		{
			var ret = new ScheduleAuditingReportData
			{
				ShiftType = Resources.AuditingReportAbsence,
				Detail = auditedAbsence.Entity.Layer.Payload.Description.Name,
				ScheduleStart = _regional.TimeZone.ConvertTimeFromUtc(auditedAbsence.Entity.Period.StartDateTime),
				ScheduleEnd = _regional.TimeZone.ConvertTimeFromUtc(auditedAbsence.Entity.Period.EndDateTime)
			};
			addCommonScheduleData(ret, auditedAbsence.Entity, auditedAbsence.RevisionEntity, auditedAbsence.Operation);
			return ret;
		}

		private void addCommonScheduleData(ScheduleAuditingReportData scheduleAuditingReportData, 
																IPersistableScheduleData auditedEntity, 
																Revision revision,
																RevisionType revisionType)
		{
			scheduleAuditingReportData.ModifiedAt = _regional.TimeZone.ConvertTimeFromUtc(revision.ModifiedAt);
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