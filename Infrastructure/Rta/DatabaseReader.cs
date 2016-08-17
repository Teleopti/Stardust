using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using log4net;
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
				.CreateSQLQuery(@"SELECT 
					PayloadId,
					StartDateTime,
					EndDateTime,
					Name,
					ShortName,
					DisplayColor, 
					BelongsToDate 
					FROM ReadModel.ScheduleProjectionReadOnly
					WHERE PersonId = :PersonId
					AND BelongsToDate BETWEEN :StartDate AND :EndDate
					ORDER BY EndDateTime ASC")
				.SetParameter("PersonId", personId)
				.SetParameter("StartDate", utcDate.AddDays(-1))
				.SetParameter("EndDate", utcDate.AddDays(1))
				.SetResultTransformer(Transformers.AliasToBean(typeof (internalModel)))
				.List<ScheduledActivity>();
		}

		private class internalModel : ScheduledActivity
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
				.CreateSQLQuery("exec[dbo].[LoadPersonOrganizationData] :now, :dataSourceId, :externalLogOn")
				.SetParameter("now", _now.UtcDateTime())
				.SetParameter("dataSourceId", dataSourceId)
				.SetParameter("externalLogOn", externalLogOn));
		}
		
		public IEnumerable<PersonOrganizationData> LoadAllPersonOrganizationData()
		{
			return readPersonOrganizationDatas(_unitOfWork.Current()
				.Session()
				.CreateSQLQuery("exec [dbo].[LoadAllPersonOrganizationData] :now")
				.SetParameter("now", _now.UtcDateTime()));
		}

		private IEnumerable<PersonOrganizationData> readPersonOrganizationDatas(IQuery query)
		{
			return query.SetResultTransformer(Transformers.AliasToBean(typeof (internalModel2)))
				.List<internalModel2>()
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

		private class internalModel2 : PersonOrganizationData
		{
			public string TimeZone { get; set; }
			public DateTime EndDate { get; set; }
		}

	}
}