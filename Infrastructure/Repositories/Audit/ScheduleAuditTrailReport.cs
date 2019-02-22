using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Envers;
using NHibernate.Envers.Query;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Reports;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Infrastructure.Repositories.Audit
{
	public class ScheduleAuditTrailReport : IScheduleAuditTrailReport
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly IUserTimeZone _timeZone;
		private readonly IGlobalSettingDataRepository _globalSettingDataRepository;

		public ScheduleAuditTrailReport(ICurrentUnitOfWork currentUnitOfWork, IUserTimeZone timeZone,
			IGlobalSettingDataRepository globalSettingDataRepository)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_timeZone = timeZone;
			_globalSettingDataRepository = globalSettingDataRepository;
		}

		public IEnumerable<SimplestPersonInfo> GetRevisionPeople(DateOnlyPeriod searchPeriod)
		{
			var startDate = searchPeriod.StartDate.AddDays(-1);
			var endDate = searchPeriod.EndDate.AddDays(1);
			const string sql = "SELECT DISTINCT p.Id, p.FirstName, p.LastName, p.EmploymentNumber "
							   + "FROM Auditing.Revision r INNER JOIN dbo.Person p ON r.ModifiedBy = p.Id where r.ModifiedAt between :startDate and :endDate";

			var commonNameDescription = getNameDescriptionSetting();
			return _currentUnitOfWork.Current().Session().CreateSQLQuery(sql)
				.AddScalar("Id", NHibernateUtil.Guid)
				.AddScalar("FirstName", NHibernateUtil.String)
				.AddScalar("LastName", NHibernateUtil.String)
				.AddScalar("EmploymentNumber", NHibernateUtil.String)
				.SetString("startDate", startDate.ToString())
				.SetString("endDate", endDate.ToString())
				.SetReadOnly(true)
				.List<object[]>()
				.Select(x => new SimplestPersonInfo
					{
						Id = (Guid) x[0],
						Name = commonNameDescription.BuildFor((string) x[1], (string) x[2], (string) x[3])
					}
				);
		}

		public IList<ScheduleAuditingReportData> Report(IPerson changedByPerson, DateOnlyPeriod changedPeriod,
			DateOnlyPeriod scheduledPeriod, int maximumResults, IList<IPerson> scheduledAgents)
		{
			var commonNameDescription = getNameDescriptionSetting();

			var auditSession = _currentUnitOfWork.Current().Session().Auditer();
			var ret = new List<ScheduleAuditingReportData>();
			var changedPeriodAgentTimeZone = changedPeriod.ToDateTimePeriod(_timeZone.TimeZone());
			var scheduledPeriodAgentTimeZone = scheduledPeriod.ToDateTimePeriod(_timeZone.TimeZone());

			var retTemp = new List<ScheduleAuditingReportData>();

			foreach (var personsBatch in scheduledAgents.Batch(500))
			{
				var personArray = personsBatch as IList<IPerson> ?? personsBatch.ToList();
				auditSession.CreateQuery().ForHistoryOf<PersonAssignment, Revision>()
					.Add(AuditEntity.RevisionProperty("ModifiedAt")
						.Between(changedPeriodAgentTimeZone.StartDateTime, changedPeriodAgentTimeZone.EndDateTime))
					.AddModifiedByIfNotNull(changedByPerson)
					.Add(AuditEntity.Property("Date").Between(scheduledPeriod.StartDate, scheduledPeriod.EndDate))
					.Add(AuditEntity.Property("Person").In(personArray))
					.AddOrder(AuditEntity.RevisionProperty("ModifiedAt").Desc())
					.SetMaxResults(maximumResults)
					.Results()
					.ForEach(assRev => retTemp.Add(createAssignmentAuditingData(assRev, commonNameDescription)));

				auditSession.CreateQuery().ForHistoryOf<PersonAbsence, Revision>()
					.Add(AuditEntity.RevisionProperty("ModifiedAt")
						.Between(changedPeriodAgentTimeZone.StartDateTime, changedPeriodAgentTimeZone.EndDateTime))
					.AddModifiedByIfNotNull(changedByPerson)
					.Add(AuditEntity.Property("Layer.Period.period.Minimum").Lt(scheduledPeriodAgentTimeZone.EndDateTime))
					.Add(AuditEntity.Property("Layer.Period.period.Maximum").Gt(scheduledPeriodAgentTimeZone.StartDateTime))
					.Add(AuditEntity.Property("Person").In(personArray))
					.AddOrder(AuditEntity.RevisionProperty("ModifiedAt").Desc())
					.SetMaxResults(maximumResults)
					.Results()
					.ForEach(absRev => retTemp.Add(createAbsenceAuditingData(absRev, commonNameDescription)));
			}

			ret.AddRange(retTemp
				.OrderByDescending(o => o.ModifiedAt)
				.Take(maximumResults));

			return ret;
		}

		private CommonNameDescriptionSetting getNameDescriptionSetting()
		{
			return _globalSettingDataRepository.FindValueByKey("CommonNameDescription", new CommonNameDescriptionSetting());
		}

		private ScheduleAuditingReportData createAssignmentAuditingData(IRevisionEntityInfo<PersonAssignment, Revision> auditedAssignment,
			CommonNameDescriptionSetting commonNameDescription)
		{
			var ret = new ScheduleAuditingReportData {ShiftType = Resources.AuditingReportShift};
			addCommonScheduleData(ret, auditedAssignment.Entity, auditedAssignment.RevisionEntity, auditedAssignment.Operation,
				commonNameDescription);

			ret.Detail = string.Empty;
			var personAssignment = auditedAssignment.Entity;

			if (personAssignment.ShiftCategory != null)
				ret.Detail = personAssignment.ShiftCategory.Description.Name;

			if (personAssignment.DayOff() != null)
			{
				ret.Detail = isOvertimeAddedOnDayOff(personAssignment)
					? Resources.Overtime
					: personAssignment.DayOff().Description.Name;
			}

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

		private ScheduleAuditingReportData createAbsenceAuditingData(IRevisionEntityInfo<PersonAbsence, Revision> auditedAbsence,
			CommonNameDescriptionSetting commonNameDescription)
		{
			var ret = new ScheduleAuditingReportData
			{
				ShiftType = Resources.AuditingReportAbsence,
				Detail = auditedAbsence.Entity.Layer.Payload.Description.Name,
				ScheduleStart = TimeZoneInfo.ConvertTimeFromUtc(auditedAbsence.Entity.Period.StartDateTime, _timeZone.TimeZone()),
				ScheduleEnd = TimeZoneInfo.ConvertTimeFromUtc(auditedAbsence.Entity.Period.EndDateTime, _timeZone.TimeZone())
			};
			addCommonScheduleData(ret, auditedAbsence.Entity, auditedAbsence.RevisionEntity, auditedAbsence.Operation,
				commonNameDescription);
			return ret;
		}

		private void addCommonScheduleData(ScheduleAuditingReportData scheduleAuditingReportData,
			IPersistableScheduleData auditedEntity, Revision revision, RevisionType revisionType,
			CommonNameDescriptionSetting commonNameDescription)
		{
			scheduleAuditingReportData.ModifiedAt = TimeZoneInfo.ConvertTimeFromUtc(revision.ModifiedAt, _timeZone.TimeZone());
			scheduleAuditingReportData.ModifiedBy = commonNameDescription.BuildFor(revision.ModifiedBy);
			scheduleAuditingReportData.ScheduledAgent = commonNameDescription.BuildFor(auditedEntity.Person);
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

		private static bool isOvertimeAddedOnDayOff(IPersonAssignment personAssignment)
		{
			var isOvertimeAdded = !personAssignment.OvertimeActivities().IsEmpty();
			return isOvertimeAdded;
		}
	}
}
