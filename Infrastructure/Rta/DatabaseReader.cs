using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
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

		public IList<ScheduleLayer> GetCurrentSchedule(Guid personId)
		{
			var utcDate = _now.UtcDateTime().Date;
			const string query = @"SELECT PayloadId,StartDateTime,EndDateTime,rta.Name,rta.ShortName,DisplayColor, BelongsToDate 
											FROM ReadModel.ScheduleProjectionReadOnly rta
											WHERE PersonId=@PersonId
											AND BelongsToDate BETWEEN @StartDate AND @EndDate";

			var layers = new List<ScheduleLayer>();
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
					var layer = new ScheduleLayer
					{
						PayloadId = reader.GetGuid(reader.GetOrdinal("PayloadId")),
						StartDateTime = reader.GetDateTime(reader.GetOrdinal("StartDateTime")),
						EndDateTime = reader.GetDateTime(reader.GetOrdinal("EndDateTime")),
						Name = reader.GetString(reader.GetOrdinal("Name")),
						ShortName = reader.GetString(reader.GetOrdinal("ShortName")),
						DisplayColor = reader.GetInt32(reader.GetOrdinal("DisplayColor")),
						BelongsToDate = new DateOnly(reader.GetDateTime(reader.GetOrdinal("BelongsToDate")))
					};
					layers.Add(layer);
				}
				reader.Close();
			}
			return layers.OrderBy(l => l.EndDateTime).ToList();
		}

		public ConcurrentDictionary<string, IEnumerable<ResolvedPerson>> ExternalLogOns()
		{
			var dictionary = new ConcurrentDictionary<string, IEnumerable<ResolvedPerson>>();
			using (var connection = new SqlConnection(_connectionStrings.Application()))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "dbo.rta_load_external_logon";
				command.Parameters.AddWithValue("@now", DateTime.UtcNow.Date);
				connection.Open();
				var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				while (reader.Read())
				{
					int loadedDataSourceId = reader.GetInt32(reader.GetOrdinal("datasource_id"));
					string originalLogOn = reader.GetString(reader.GetOrdinal("acd_login_original_id"));
					Guid personId = reader.GetGuid(reader.GetOrdinal("person_code"));
					Guid businessUnitId = reader.GetGuid(reader.GetOrdinal("business_unit_code"));

					var lookupKey = string.Format(CultureInfo.InvariantCulture, "{0}|{1}", loadedDataSourceId, originalLogOn).ToUpper(CultureInfo.InvariantCulture);
					var personWithBusinessUnit = new ResolvedPerson
					{
						PersonId = personId,
						BusinessUnitId = businessUnitId
					};

					IEnumerable<ResolvedPerson> list;
					if (dictionary.TryGetValue(lookupKey, out list))
					{
						((ICollection<ResolvedPerson>)list).Add(personWithBusinessUnit);
					}
					else
					{
						var newCollection = new Collection<ResolvedPerson> { personWithBusinessUnit };
						dictionary.AddOrUpdate(lookupKey, newCollection, (s, units) => newCollection);
					}
				}
				reader.Close();
			}
			return dictionary;
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

		private const string loadAllPersonsBuSiteTeam = "exec [dbo].[LoadAllPersonsCurrentBuSiteTeam] @now";
		public IEnumerable<PersonOrganizationData> PersonOrganizationData()
		{
			var ret = new List<PersonOrganizationData>();
			using (var conn = new SqlConnection(_connectionStrings.Application()))
			{
				conn.Open();
				using (var cmd = new SqlCommand(loadAllPersonsBuSiteTeam, conn))
				{
					cmd.Parameters.AddWithValue("@now", _now.UtcDateTime());
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							ret.Add(new PersonOrganizationData
							{
								PersonId = reader.GetGuid(reader.GetOrdinal("PersonId")),
								TeamId = reader.GetGuid(reader.GetOrdinal("TeamId")),
								SiteId = reader.GetGuid(reader.GetOrdinal("SiteId"))
							});
						}
					}
				}
			}
			return ret;
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

		private static IEnumerable<PersonOrganizationData> readPersonOrganizationDatas(SqlCommand cmd)
		{
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					yield return new PersonOrganizationData
					{
						PersonId = reader.GetGuid(reader.GetOrdinal("PersonId")),
						TeamId = reader.GetGuid(reader.GetOrdinal("TeamId")),
						SiteId = reader.GetGuid(reader.GetOrdinal("SiteId")),
						BusinessUnitId = reader.GetGuid(reader.GetOrdinal("BusinessUnitId"))
					};
				}
			}
		}
	}
}