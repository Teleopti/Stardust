using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server.Resolvers;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Rta.Server
{
	public class DatabaseReader : IDatabaseReader
	{
		private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
		private readonly IDatabaseConnectionStringHandler _databaseConnectionStringHandler;
        private readonly IActualAgentStateCache _actualAgentStateCache;
	    private readonly INow _now;
	    private static readonly ILog LoggingSvc = LogManager.GetLogger(typeof(IDatabaseReader));

	    public DatabaseReader(IDatabaseConnectionFactory databaseConnectionFactory,
		    IDatabaseConnectionStringHandler databaseConnectionStringHandler,
		    IActualAgentStateCache actualAgentStateCache,
		    INow now)
        {
            _databaseConnectionFactory = databaseConnectionFactory;
            _databaseConnectionStringHandler = databaseConnectionStringHandler;
            _actualAgentStateCache = actualAgentStateCache;
	        _now = now;
        }

        public IList<ScheduleLayer> GetReadModel(Guid personId)
        {
	        var utcDate = _now.UtcDateTime().Date;
	        var query = string.Format(CultureInfo.InvariantCulture,
		        @"SELECT PayloadId,StartDateTime,EndDateTime,rta.Name,rta.ShortName,DisplayColor 
											FROM ReadModel.ScheduleProjectionReadOnly rta
											WHERE PersonId='{0}'
											AND BelongsToDate BETWEEN '{1}' AND '{2}'", personId,
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
							DisplayColor = reader.GetInt32(reader.GetOrdinal("DisplayColor"))
						};
					layers.Add(layer);
				}
				reader.Close();
			}
			return layers.OrderBy(l => l.EndDateTime).ToList();
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
			"CA2100:Review SQL queries for security vulnerabilities")]
		public IActualAgentState LoadOldState(Guid personToLoad)
		{
			LoggingSvc.DebugFormat("Getting old state for person: {0}", personToLoad);

		    IActualAgentState notFlushedState;
		    if (_actualAgentStateCache.TryGetLatestState(personToLoad, out notFlushedState))
		    {
		        return notFlushedState;
		    }

		    var query =
				string.Format(
					"SELECT AlarmId, StateStart, ScheduledId, ScheduledNextId, StateId, ScheduledNextId, NextStart, PlatformTypeId, StateCode, BatchId, OriginalDataSourceId, AlarmStart FROM RTA.ActualAgentState WHERE PersonId ='{0}'", personToLoad);
			using (
				var connection =
					_databaseConnectionFactory.CreateConnection(
						_databaseConnectionStringHandler.DataStoreConnectionString()))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				command.CommandText = query;
				connection.Open();
				using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
				{
					while (reader.Read())
					{
						var agentState = new ActualAgentState
							{
								PlatformTypeId = reader.GetGuid(reader.GetOrdinal("PlatformTypeId")),
								StateCode = reader.GetString(reader.GetOrdinal("StateCode")),
								StateId = reader.GetGuid(reader.GetOrdinal("StateId")),
								ScheduledId = reader.GetGuid(reader.GetOrdinal("ScheduledId")),
								AlarmId = reader.GetGuid(reader.GetOrdinal("AlarmId")),
								ScheduledNextId = reader.GetGuid(reader.GetOrdinal("ScheduledNextId")),
								StateStart = reader.GetDateTime(reader.GetOrdinal("StateStart")),
								NextStart = reader.GetDateTime(reader.GetOrdinal("NextStart")),
								BatchId = !reader.IsDBNull(reader.GetOrdinal("BatchId"))
											  ? reader.GetDateTime(reader.GetOrdinal("BatchId"))
											  : (DateTime?)null,
								OriginalDataSourceId = !reader.IsDBNull(reader.GetOrdinal("OriginalDataSourceId"))
														   ? reader.GetString(reader.GetOrdinal("OriginalDataSourceId"))
														   : "",
								AlarmStart = reader.GetDateTime(reader.GetOrdinal("AlarmStart"))
							};
						LoggingSvc.DebugFormat("Found old state for person: {0}, AgentState: {1}", personToLoad, agentState);
						return agentState;
					}
				}
			}
			LoggingSvc.DebugFormat("Found no state for person: {0}", personToLoad);
			return null;
		}

		public IList<IActualAgentState> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId)
		{
			var missingUsers = new List<IActualAgentState>();
			using (var connection = _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.DataStoreConnectionString()))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "[RTA].[rta_get_last_batch]";
				command.Parameters.Add(new SqlParameter("@datasource_id", dataSourceId));
				command.Parameters.Add(new SqlParameter("@batch_id", batchId));

				connection.Open();
				var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				while (reader != null && reader.Read())
				{
					missingUsers.Add(new ActualAgentState
						{
							BusinessUnit = reader.GetGuid(reader.GetOrdinal("BusinessUnitId")),
							PersonId = reader.GetGuid(reader.GetOrdinal("PersonId")),
							StateCode = reader.GetString(reader.GetOrdinal("StateCode")),
							PlatformTypeId = reader.GetGuid(reader.GetOrdinal("PlatformTypeId")),
							State = reader.GetString(reader.GetOrdinal("State")),
							StateId = reader.GetGuid(reader.GetOrdinal("StateId")),
							Scheduled = reader.GetString(reader.GetOrdinal("Scheduled")),
							ScheduledId = reader.GetGuid(reader.GetOrdinal("ScheduledId")),
							StateStart = reader.GetDateTime(reader.GetOrdinal("StateStart")),
							ScheduledNext = reader.GetString(reader.GetOrdinal("ScheduledNext")),
							ScheduledNextId = reader.GetGuid(reader.GetOrdinal("ScheduledNextId")),
							NextStart = reader.GetDateTime(reader.GetOrdinal("NextStart")),
						});
				}
				if (reader != null)
					reader.Close();
			}

			return missingUsers;
		}

		public ConcurrentDictionary<Tuple<string,Guid,Guid>, List<RtaStateGroupLight>> StateGroups()
		{
			const string query =
				@"SELECT s.Id StateId, s.Name StateName, s.PlatformTypeId,s.StateCode, sg.Id StateGroupId, sg.Name StateGroupName, BusinessUnit BusinessUnitId, sg.IsLogOutState 
								FROM RtaStateGroup sg
								INNER JOIN RtaState s ON s.Parent = sg.Id 
								WHERE sg.IsDeleted = 0 
								ORDER BY StateCode";
			var stateGroups = new List<RtaStateGroupLight>();
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
					var rtaStateGroupLight = new RtaStateGroupLight
												{
													StateGroupName = reader.GetString(reader.GetOrdinal("StateGroupName")),
													BusinessUnitId = reader.GetGuid(reader.GetOrdinal("BusinessUnitId")),
													PlatformTypeId = reader.GetGuid(reader.GetOrdinal("PlatformTypeId")),
													StateCode = reader.GetString(reader.GetOrdinal("StateCode")).ToUpper(CultureInfo.InvariantCulture),
													StateGroupId = reader.GetGuid(reader.GetOrdinal("StateGroupId")),
													StateId = reader.GetGuid(reader.GetOrdinal("StateId")),
													StateName = reader.GetString(reader.GetOrdinal("StateName")),
													IsLogOutState = reader.GetBoolean(reader.GetOrdinal("IsLogOutState"))
												};
					stateGroups.Add(rtaStateGroupLight);
				}
				reader.Close();
			}
            return new ConcurrentDictionary<Tuple<string, Guid, Guid>, List<RtaStateGroupLight>>(stateGroups.GroupBy(s => new Tuple<string, Guid, Guid>(s.StateCode, s.PlatformTypeId, s.BusinessUnitId)).ToDictionary(g => g.Key, g => g.ToList()));
		}

		public ConcurrentDictionary<Tuple<Guid,Guid,Guid>, List<RtaAlarmLight>> ActivityAlarms()
		{
			var stateGroups = new List<RtaAlarmLight>();
			using (var connection = _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.AppConnectionString()))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "[dbo].[ActivityAlarmMapping]";
				connection.Open();
				var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				while (reader.Read())
				{
					var rtaAlarmLight = new RtaAlarmLight
						{
							StateGroupName = !reader.IsDBNull(reader.GetOrdinal("StateGroupName"))
												 ? reader.GetString(reader.GetOrdinal("StateGroupName"))
												 : "",
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
				new ConcurrentDictionary<Tuple<Guid,Guid,Guid>, List<RtaAlarmLight>>(stateGroups.GroupBy(g => new Tuple<Guid,Guid,Guid>(g.ActivityId,g.StateGroupId,g.BusinessUnit))
																			   .ToDictionary(k => k.Key, v => v.ToList()));
		}

		public ConcurrentDictionary<string, IEnumerable<PersonWithBusinessUnit>> LoadAllExternalLogOns()
		{
			var dictionary = new ConcurrentDictionary<string, IEnumerable<PersonWithBusinessUnit>>();
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
					var personWithBusinessUnit = new PersonWithBusinessUnit
					{
						PersonId = personId,
						BusinessUnitId = businessUnitId
					};

					IEnumerable<PersonWithBusinessUnit> list;
					if (dictionary.TryGetValue(lookupKey, out list))
					{
						((ICollection<PersonWithBusinessUnit>)list).Add(personWithBusinessUnit);
					}
					else
					{
						var newCollection = new Collection<PersonWithBusinessUnit> { personWithBusinessUnit };
						dictionary.AddOrUpdate(lookupKey, newCollection, (s, units) => newCollection);
					}
				}
				reader.Close();
			}
			return dictionary;
		}

		public ConcurrentDictionary<string, int> LoadDatasources()
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
						LoggingSvc.WarnFormat("There is already a source defined with the id = {0}",
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
}