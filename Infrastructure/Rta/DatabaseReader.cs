﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class DatabaseReader : IDatabaseReader
	{
		private readonly IConnectionStrings _connectionStrings;
		private readonly INow _now;
		private static readonly ILog LoggingSvc = LogManager.GetLogger(typeof(IDatabaseReader));

		public DatabaseReader(
			IConnectionStrings connectionStrings,
			INow now)
		{
			_connectionStrings = connectionStrings;
			_now = now;
		}

		public IList<ScheduledActivity> GetCurrentSchedule(Guid personId)
		{
			var utcDate = _now.UtcDateTime().Date;
			const string query = @"SELECT PayloadId,StartDateTime,EndDateTime,rta.Name,rta.ShortName,DisplayColor, BelongsToDate 
											FROM ReadModel.ScheduleProjectionReadOnly rta
											WHERE PersonId=@PersonId
											AND BelongsToDate BETWEEN @StartDate AND @EndDate";

			var layers = new List<ScheduledActivity>();
			using (var connection = new SqlConnection(_connectionStrings.Application()))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				command.CommandText = query;
				command.Parameters.AddWithValue("@PersonId", personId);
				command.Parameters.AddWithValue("@StartDate", utcDate.AddDays(-1));
				command.Parameters.AddWithValue("@EndDate", utcDate.AddDays(1));
				connection.Open();
				var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				while (reader.Read())
				{
					var layer = new ScheduledActivity
					{
						PayloadId = reader.GetGuid(reader.GetOrdinal("PayloadId")),
						StartDateTime = reader.GetDateTime(reader.GetOrdinal("StartDateTime")),
						EndDateTime = reader.GetDateTime(reader.GetOrdinal("EndDateTime")),
						Name = reader.String("Name"),
						ShortName = reader.String("ShortName"),
						DisplayColor = reader.GetInt32(reader.GetOrdinal("DisplayColor")),
						BelongsToDate = new DateOnly(reader.GetDateTime(reader.GetOrdinal("BelongsToDate")))
					};
					layers.Add(layer);
				}
				reader.Close();
			}
			return layers.OrderBy(l => l.EndDateTime).ToList();
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
			using (var conn = new SqlConnection(_connectionStrings.Application()))
			{
				conn.Open();
				using (var cmd = new SqlCommand("exec [dbo].[LoadPersonOrganizationData] @now, @dataSourceId, @externalLogOn", conn))
				{
					cmd.Parameters.AddWithValue("@now", _now.UtcDateTime());
					cmd.Parameters.AddWithValue("@dataSourceId", dataSourceId);
					cmd.Parameters.AddWithValue("@externalLogOn", externalLogOn);

					return readPersonOrganizationDatas(cmd).ToArray();
				}
			}
		}
		
		public IEnumerable<PersonOrganizationData> LoadAllPersonOrganizationData()
		{
			using (var conn = new SqlConnection(_connectionStrings.Application()))
			{
				conn.Open();
				using (var cmd = new SqlCommand("exec [dbo].[LoadAllPersonOrganizationData] @now", conn))
				{
					cmd.Parameters.AddWithValue("@now", _now.UtcDateTime());
					return readPersonOrganizationDatas(cmd).ToArray();
				}
			}

		}

		private IEnumerable<PersonOrganizationData> readPersonOrganizationDatas(SqlCommand cmd)
		{
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					var timeZoneValue = reader.GetString(reader.GetOrdinal("TimeZone"));
					var endDateValue = reader.GetDateTime(reader.GetOrdinal("EndDate"));

					var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneValue);
					var terminatedAt = endDateValue.AddDays(1);
					terminatedAt = TimeZoneInfo.ConvertTimeToUtc(terminatedAt, timeZone);

					if (terminatedAt > _now.UtcDateTime())
					{
						yield return new PersonOrganizationData
						{
							PersonId = reader.GetGuid(reader.GetOrdinal("PersonId")),
							TeamId = reader.GetGuid(reader.GetOrdinal("TeamId")),
							SiteId = reader.GetGuid(reader.GetOrdinal("SiteId")),
							BusinessUnitId = reader.GetGuid(reader.GetOrdinal("BusinessUnitId")),
						};
					}
				}
			}
		}
	}
}