using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using log4net;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class DatabaseReader : IDatabaseReader
	{
		private readonly IConnectionStrings _connectionStrings;
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly INow _now;
		private static readonly ILog LoggingSvc = LogManager.GetLogger(typeof(IDatabaseReader));

		public DatabaseReader(
			IConnectionStrings connectionStrings,
			ICurrentUnitOfWork unitOfWork,
			INow now)
		{
			_connectionStrings = connectionStrings;
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
			var dictionary = new ConcurrentDictionary<string, int>();
			using (var connection = new SqlConnection(_connectionStrings.Analytics()))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "RTA.rta_load_datasources";
				connection.Open();
				var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				while (reader.Read())
				{
					var loadedSourceId = reader["source_id"];
					int loadedDataSourceId = reader.GetInt16(reader.GetOrdinal("datasource_id")); //This one cannot be null as it's the PK of the table
					if (loadedSourceId == DBNull.Value)
					{
						LoggingSvc.WarnFormat("No source id is defined for data source = {0}", loadedDataSourceId);
						continue;
					}
					var loadedSourceIdAsString = (string)loadedSourceId;
					if (dictionary.ContainsKey(loadedSourceIdAsString))
					{
						LoggingSvc.DebugFormat("There is already a source defined with the id = {0}",
												 loadedSourceIdAsString);
						continue;
					}
					dictionary.AddOrUpdate(loadedSourceIdAsString, loadedDataSourceId, (s, i) => loadedDataSourceId);
				}
				reader.Close();
			}
			return dictionary;
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