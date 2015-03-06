using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class DatabaseReader : IDatabaseReader
	{
		private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
		private readonly IDatabaseConnectionStringHandler _databaseConnectionStringHandler;
		private readonly INow _now;
		private static readonly ILog LoggingSvc = LogManager.GetLogger(typeof(IDatabaseReader));

		public DatabaseReader(
			IDatabaseConnectionFactory databaseConnectionFactory,
			IDatabaseConnectionStringHandler databaseConnectionStringHandler,
			INow now)
		{
			_databaseConnectionFactory = databaseConnectionFactory;
			_databaseConnectionStringHandler = databaseConnectionStringHandler;
			_now = now;
		}

		public IList<ScheduleLayer> GetCurrentSchedule(Guid personId)
		{
			var utcDate = _now.UtcDateTime().Date;
			var query = string.Format(CultureInfo.InvariantCulture,
				@"SELECT PayloadId,StartDateTime,EndDateTime,rta.Name,rta.ShortName,DisplayColor, BelongsToDate 
											FROM ReadModel.ScheduleProjectionReadOnly rta
											WHERE PersonId='{0}'
											AND BelongsToDate BETWEEN '{1}' AND '{2}'",
				personId,
				utcDate.AddDays(-1),
				utcDate.AddDays(1));
			var layers = new List<ScheduleLayer>();
			using (var connection = _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.AppConnectionString()))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				command.CommandText = query;
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

		public AgentStateReadModel GetCurrentActualAgentState(Guid personId)
		{
			LoggingSvc.DebugFormat("Getting old state for person: {0}", personId);

			var agentState = queryActualAgentStates(personId).FirstOrDefault();

			if (agentState == null)
				LoggingSvc.DebugFormat("Found no state for person: {0}", personId);
			else
				LoggingSvc.DebugFormat("Found old state for person: {0}, AgentState: {1}", personId, agentState);

			return agentState;
		}

		public IEnumerable<AgentStateReadModel> GetActualAgentStates()
		{
			return queryActualAgentStates(null);
		}

		private IEnumerable<AgentStateReadModel> queryActualAgentStates(Guid? personId)
		{
			var query = "SELECT * FROM RTA.ActualAgentState";
			if (personId.HasValue)
				query = string.Format("SELECT * FROM RTA.ActualAgentState WHERE PersonId ='{0}'", personId);
			using (
				var connection =
					_databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.DataStoreConnectionString()))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				command.CommandText = query;
				connection.Open();
				using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
				{
					while (reader.Read())
					{
						yield return new AgentStateReadModel
						{
							PlatformTypeId = reader.Guid("PlatformTypeId"),
							BusinessUnitId = reader.NullableGuid("BusinessUnitId") ?? Guid.Empty,
							StateCode = reader.String("StateCode"),
							StateId = reader.NullableGuid("StateId"),
							State = reader.String("State"),
							ScheduledId = reader.NullableGuid("ScheduledId"),
							Scheduled = reader.String("Scheduled"),
							ScheduledNextId = reader.NullableGuid("ScheduledNextId"),
							ScheduledNext = reader.String("ScheduledNext"),
							ReceivedTime = reader.DateTime("ReceivedTime"),
							Color = reader.NullableInt("Color"),
							AlarmId = reader.NullableGuid("AlarmId"),
							AlarmName = reader.String("AlarmName"),
							StateStart = reader.NullableDateTime("StateStart"),
							NextStart = reader.NullableDateTime("NextStart"),
							BatchId = reader.NullableDateTime("BatchId"),
							OriginalDataSourceId = reader.String("OriginalDataSourceId"),
							AlarmStart = reader.NullableDateTime("AlarmStart"),
							PersonId = reader.Guid("PersonId"),
							StaffingEffect = reader.NullableDouble("StaffingEffect"),
							TeamId = reader.NullableGuid("TeamId"),
							SiteId = reader.NullableGuid("SiteId")
						};
					}
				}
			}
		}

		public IEnumerable<AgentStateReadModel> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId)
		{
			var missingUsers = new List<AgentStateReadModel>();
			using (var connection = _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.DataStoreConnectionString()))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "[RTA].[rta_get_last_batch]";
				command.Parameters.Add(new SqlParameter("@datasource_id", dataSourceId));
				command.Parameters.Add(new SqlParameter("@batch_id", batchId));

				connection.Open();
				var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				while (reader.Read())
				{
					missingUsers.Add(new AgentStateReadModel
						{
							BusinessUnitId = reader.GetGuid(reader.GetOrdinal("BusinessUnitId")),
							PersonId = reader.GetGuid(reader.GetOrdinal("PersonId")),
							StateCode = reader.String("StateCode"),
							PlatformTypeId = reader.GetGuid(reader.GetOrdinal("PlatformTypeId")),
							State = reader.String("State"),
							StateId = reader.NullableGuid("StateId"),
							Scheduled = reader.String("Scheduled"),
							ScheduledId = reader.NullableGuid("ScheduledId"),
							StateStart = reader.NullableDateTime("StateStart"),
							ScheduledNext = reader.String("ScheduledNext"),
							ScheduledNextId = reader.NullableGuid("ScheduledNextId"),
							NextStart = reader.NullableDateTime("NextStart"),
						});
				}
			}

			return missingUsers;
		}

		public IEnumerable<StateCodeInfo> StateCodeInfos()
		{
			const string query =
				@"SELECT s.Id StateId, s.Name StateName, s.PlatformTypeId,s.StateCode, sg.Id StateGroupId, sg.Name StateGroupName, BusinessUnit BusinessUnitId, sg.IsLogOutState 
								FROM RtaStateGroup sg
								INNER JOIN RtaState s ON s.Parent = sg.Id 
								WHERE sg.IsDeleted = 0 
								ORDER BY StateCode";
			var stateCodes = new List<StateCodeInfo>();
			using (
				var connection = _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.AppConnectionString())
				)
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				command.CommandText = query;
				connection.Open();
				var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				while (reader.Read())
				{
					stateCodes.Add(new StateCodeInfo
					{
						StateGroupName = reader.GetString(reader.GetOrdinal("StateGroupName")),
						BusinessUnitId = reader.GetGuid(reader.GetOrdinal("BusinessUnitId")),
						PlatformTypeId = reader.GetGuid(reader.GetOrdinal("PlatformTypeId")),
						StateCode = reader.GetString(reader.GetOrdinal("StateCode")).ToUpper(CultureInfo.InvariantCulture),
						StateGroupId = reader.GetGuid(reader.GetOrdinal("StateGroupId")),
						StateName = reader.GetString(reader.GetOrdinal("StateName")),
						IsLogOutState = reader.GetBoolean(reader.GetOrdinal("IsLogOutState"))
					});
				}
				reader.Close();
			}
			return stateCodes;
		}

		public ConcurrentDictionary<Tuple<Guid, Guid, Guid>, List<AlarmMappingInfo>> AlarmMappingInfos()
		{
			var stateGroups = new List<AlarmMappingInfo>();
			using (var connection = _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.AppConnectionString()))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "[dbo].[ActivityAlarmMapping]";
				connection.Open();
				var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				while (reader.Read())
				{
					var rtaAlarmLight = new AlarmMappingInfo
						{
							StateGroupId = !reader.IsDBNull(reader.GetOrdinal("StateGroupId"))
												 ? reader.GetGuid(reader.GetOrdinal("StateGroupId"))
												 : Guid.Empty,
							ActivityId = !reader.IsDBNull(reader.GetOrdinal("ActivityId"))
											 ? reader.GetGuid(reader.GetOrdinal("ActivityId"))
											 : Guid.Empty,
							DisplayColor = !reader.IsDBNull(reader.GetOrdinal("DisplayColor"))
												 ? reader.GetInt32(reader.GetOrdinal("DisplayColor"))
												 : 0,
							StaffingEffect = !reader.IsDBNull(reader.GetOrdinal("StaffingEffect"))
												 ? reader.GetDouble(reader.GetOrdinal("StaffingEffect"))
												 : 0,
							AlarmTypeId = !reader.IsDBNull(reader.GetOrdinal("AlarmTypeId"))
												? reader.GetGuid(reader.GetOrdinal("AlarmTypeId"))
												: Guid.Empty,
							Name = !reader.IsDBNull(reader.GetOrdinal("Name"))
										 ? reader.GetString(reader.GetOrdinal("Name"))
										 : "",
							ThresholdTime = !reader.IsDBNull(reader.GetOrdinal("ThresholdTime"))
												? reader.GetInt64(reader.GetOrdinal("ThresholdTime"))
												: 0,
							BusinessUnit = reader.GetGuid(reader.GetOrdinal("BusinessUnit"))
						};
					stateGroups.Add(rtaAlarmLight);
				}
				reader.Close();
			}
			return
				new ConcurrentDictionary<Tuple<Guid, Guid, Guid>, List<AlarmMappingInfo>>(stateGroups.GroupBy(g => new Tuple<Guid, Guid, Guid>(g.ActivityId, g.StateGroupId, g.BusinessUnit))
																				 .ToDictionary(k => k.Key, v => v.ToList()));
		}

		public ConcurrentDictionary<string, IEnumerable<ResolvedPerson>> ExternalLogOns()
		{
			var dictionary = new ConcurrentDictionary<string, IEnumerable<ResolvedPerson>>();
			using (var connection = _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.AppConnectionString()))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "dbo.rta_load_external_logon";
				command.Parameters.Add(new SqlParameter("@now", DateTime.UtcNow.Date));
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
			using (var connection = _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.DataStoreConnectionString()))
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
						LoggingSvc.InfoFormat("There is already a source defined with the id = {0}",
												 loadedSourceIdAsString);
						continue;
					}
					dictionary.AddOrUpdate(loadedSourceIdAsString, loadedDataSourceId, (s, i) => loadedDataSourceId);
				}
				reader.Close();
			}
			return dictionary;
		}
	}

	public static class SqlDataReaderExtensions
	{

		public static Guid Guid(this IDataReader reader, string name)
		{
			var ordinal = reader.GetOrdinal(name);
			return reader.GetGuid(ordinal);
		}

		public static int? NullableInt(this IDataReader reader, string name)
		{
			var ordinal = reader.GetOrdinal(name);
			return !reader.IsDBNull(ordinal)
				? reader.GetInt32(ordinal)
				: (int?)null;
		}

		public static double? NullableDouble(this IDataReader reader, string name)
		{
			var ordinal = reader.GetOrdinal(name);
			return !reader.IsDBNull(ordinal)
				? reader.GetDouble(ordinal)
				: (double?)null;
		}

		public static string String(this IDataReader reader, string name)
		{
			var ordinal = reader.GetOrdinal(name);
			return !reader.IsDBNull(ordinal)
				? reader.GetString(ordinal)
				: null;
		}

		public static Guid? NullableGuid(this IDataReader reader, string name)
		{
			var ordinal = reader.GetOrdinal(name);
			return !reader.IsDBNull(ordinal)
				? reader.GetGuid(ordinal)
				: (Guid?)null;
		}

		public static DateTime? NullableDateTime(this IDataReader reader, string name)
		{
			var ordinal = reader.GetOrdinal(name);
			return !reader.IsDBNull(ordinal)
				? reader.GetDateTime(ordinal)
				: (DateTime?)null;
		}

		public static DateTime DateTime(this IDataReader reader, string name)
		{
			var ordinal = reader.GetOrdinal(name);
			return reader.GetDateTime(ordinal);
		}

	}
}