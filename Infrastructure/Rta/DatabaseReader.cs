using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class DatabaseReader : IDatabaseReader
	{
		private readonly ICurrentAnalyticsUnitOfWork _analyticsUnitOfWork;
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly INow _now;

		public DatabaseReader(
			ICurrentAnalyticsUnitOfWork analyticsUnitOfWork,
			ICurrentUnitOfWork unitOfWork,
			INow now)
		{
			_analyticsUnitOfWork = analyticsUnitOfWork;
			_unitOfWork = unitOfWork;
			_now = now;
		}

		public IList<ScheduledActivity> GetCurrentSchedule(Guid personId)
		{
			var utcDate = _now.UtcDateTime().Date;
			return _unitOfWork.Current()
				.Session()
				.CreateSQLQuery(scheduleQuery("PersonId = :PersonId"))
				.SetParameter("PersonId", personId)
				.SetParameter("StartDate", utcDate.AddDays(-1))
				.SetParameter("EndDate", utcDate.AddDays(1))
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalScheduledActivity)))
				.List<ScheduledActivity>();
		}

		public IEnumerable<ScheduledActivity> GetCurrentSchedules(IEnumerable<Guid> personIds)
		{
			var utcDate = _now.UtcDateTime().Date;
			return _unitOfWork.Current()
				.Session()
				.CreateSQLQuery(scheduleQuery("PersonId IN (:PersonIds)"))
				.SetParameterList("PersonIds", personIds)
				.SetParameter("StartDate", utcDate.AddDays(-1))
				.SetParameter("EndDate", utcDate.AddDays(1))
				.SetResultTransformer(Transformers.AliasToBean(typeof(internalScheduledActivity)))
				.List<ScheduledActivity>();
		}

		private static string scheduleQuery(string constraint)
		{
			return $@"
SELECT
	PersonId,
	PayloadId,
	StartDateTime,
	EndDateTime,
	Name,
	ShortName,
	DisplayColor, 
	BelongsToDate 
FROM ReadModel.ScheduleProjectionReadOnly
WHERE 
	{constraint} AND
	BelongsToDate BETWEEN :StartDate AND :EndDate
ORDER BY EndDateTime ASC";
		}

		private class internalScheduledActivity : ScheduledActivity
		{
			public new DateTime BelongsToDate { set { base.BelongsToDate = new DateOnly(value); } }
			public new DateTime StartDateTime {  set { base.StartDateTime = DateTime.SpecifyKind(value, DateTimeKind.Utc); } }
			public new DateTime EndDateTime {  set { base.EndDateTime = DateTime.SpecifyKind(value, DateTimeKind.Utc); } }
		}

		public ConcurrentDictionary<string, int> Datasources()
		{
			var datasources = _analyticsUnitOfWork.Current().Session()
				.CreateSQLQuery("SELECT datasource_id, source_id FROM mart.sys_datasource")
				.SetResultTransformer(Transformers.AliasToBean(typeof (datasource)))
				.List<datasource>()
				.GroupBy(x => x.source_id, (key, g) => g.First());

			return new ConcurrentDictionary<string, int>(datasources
				.ToDictionary(datasource => datasource.source_id, datasource => datasource.datasource_id));
		}

		private class datasource
		{
			public int datasource_id { get; set; }
			public string source_id { get; set; }
		}

		public IEnumerable<PersonOrganizationData> LoadPersonOrganizationData(int dataSourceId, string externalLogOn)
		{
			return readPersonOrganizationDatas(_unitOfWork.Current()
				.Session()
				.CreateSQLQuery($@"
{personOrganizationDataQuery}
AND v.AcdLogOnOriginalId = :externalLogOn
AND v.DataSourceId = :dataSourceId")
				.SetParameter("now", _now.UtcDateTime())
				.SetParameter("dataSourceId", dataSourceId)
				.SetParameter("externalLogOn", externalLogOn));
		}

		public IEnumerable<PersonOrganizationData> LoadPersonOrganizationDatas(int dataSourceId, IEnumerable<string> externalLogOns)
		{
			return readPersonOrganizationDatas(_unitOfWork.Current()
				.Session()
				.CreateSQLQuery($@"
{personOrganizationDataQuery}
AND v.AcdLogOnOriginalId IN (:externalLogOns)
AND v.DataSourceId = :dataSourceId")
				.SetParameter("now", _now.UtcDateTime())
				.SetParameter("dataSourceId", dataSourceId)
				.SetParameterList("externalLogOns", externalLogOns));
		}

		public IEnumerable<PersonOrganizationData> LoadAllPersonOrganizationData()
		{
			return readPersonOrganizationDatas(_unitOfWork.Current()
				.Session()
				.CreateSQLQuery(personOrganizationDataQuery)
				.SetParameter("now", _now.UtcDateTime()));
		}

		private IEnumerable<PersonOrganizationData> readPersonOrganizationDatas(IQuery query)
		{
			return query.SetResultTransformer(Transformers.AliasToBean(typeof (internalPersonOrganizationData)))
				.List<internalPersonOrganizationData>()
				.Where(x =>
				{
					var timeZoneValue = x.TimeZone;
					var endDateValue = x.EndDate;

					var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneValue);
					var terminatedAt = endDateValue.AddDays(1);
					terminatedAt = TimeZoneInfo.ConvertTimeToUtc(terminatedAt, timeZone);

					return terminatedAt > _now.UtcDateTime();
				}).ToList();
		}

		private class internalPersonOrganizationData : PersonOrganizationData
		{
			public string TimeZone { get; set; }
			public DateTime EndDate { get; set; }
		}

		private string personOrganizationDataQuery = @"
SELECT
	v.AcdLogOnOriginalId AS UserCode,
	v.PersonId,
	v.TeamId,
	v.SiteId,
	v.BusinessUnitId,
	v.TimeZone,
	v.EndDate
FROM
	dbo.v_PersonOrganizationData v WITH (NOEXPAND)
WHERE
	:now >= v.StartDate AND
	DATEADD(DAY, -2, :now) <= v.EndDate";
	}
}