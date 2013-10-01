using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Rta.Server
{
	public interface IDatabaseHandler
	{
		IList<ScheduleLayer> CurrentLayerAndNext(DateTime onTime, IList<ScheduleLayer> layers);
		IActualAgentState LoadOldState(Guid personToLoad);
		ConcurrentDictionary<string, List<RtaStateGroupLight>> StateGroups();
		ConcurrentDictionary<Guid, List<RtaAlarmLight>> ActivityAlarms();
		void AddOrUpdate(IList<IActualAgentState> states);
		IList<ScheduleLayer> GetReadModel(Guid personId);
		RtaStateGroupLight AddAndGetNewRtaState(string stateCode, Guid platformTypeId);
		IList<IActualAgentState> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId);
	}

	public class DatabaseHandler : IDatabaseHandler
	{
		private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
		private readonly IDatabaseConnectionStringHandler _databaseConnectionStringHandler;
		private static readonly ILog LoggingSvc = LogManager.GetLogger(typeof(IDatabaseHandler));

		public DatabaseHandler(IDatabaseConnectionFactory databaseConnectionFactory,
		                                   IDatabaseConnectionStringHandler databaseConnectionStringHandler)
		{
			_databaseConnectionFactory = databaseConnectionFactory;
			_databaseConnectionStringHandler = databaseConnectionStringHandler;
		}

		public IList<ScheduleLayer> CurrentLayerAndNext(DateTime onTime, IList<ScheduleLayer> layers)
		{
			LoggingSvc.Info("Finding current layer and next");
			if (!layers.Any())
				return new List<ScheduleLayer> {null, null};
			var scheduleLayers = layers.Where(l => l.EndDateTime > onTime);
			var enumerable = scheduleLayers as IList<ScheduleLayer> ?? scheduleLayers.ToArray();
			ScheduleLayer scheduleLayer = null;
			ScheduleLayer nextLayer = null;
			if (enumerable.Any())
				scheduleLayer = enumerable[0];
			
			// no layer now
			if (scheduleLayer != null && scheduleLayer.StartDateTime > onTime)
			{
				nextLayer = scheduleLayer;
				scheduleLayer = null;
			}

			if (nextLayer == null && enumerable.Count() > 1)
				nextLayer = enumerable[1];

			if (scheduleLayer != null && nextLayer != null)
				//scheduleLayer is the last in assignment
				if (scheduleLayer.EndDateTime != nextLayer.StartDateTime)
					nextLayer = null;

			if (scheduleLayer != null)
				LoggingSvc.InfoFormat(CultureInfo.InvariantCulture, "Current layer = Name: {0}, StartTime: {1}, EndTime: {2}", scheduleLayer.Name, scheduleLayer.StartDateTime, scheduleLayer.EndDateTime);
			if (nextLayer != null)
				LoggingSvc.InfoFormat(CultureInfo.InvariantCulture, "Next layer = Name: {0}, StartTime: {1}, EndTime: {2}", nextLayer.Name, nextLayer.StartDateTime, nextLayer.EndDateTime);
			return new List<ScheduleLayer> {scheduleLayer, nextLayer};
		}

		public IList<ScheduleLayer> GetReadModel(Guid personId)
		{
			var idString = string.Format(CultureInfo.InvariantCulture, personId.ToString());
			var query = string.Format(CultureInfo.InvariantCulture,
			                          @"SELECT PayloadId,StartDateTime,EndDateTime,rta.Name,rta.ShortName,DisplayColor 
											FROM ReadModel.v_ScheduleProjectionReadOnlyRTA rta
											WHERE PersonId='{0}'", idString);
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
			LoggingSvc.InfoFormat("Getting old state for person: {0}", personToLoad);
			var personIdString = personToLoad.ToString();
			var query =
				string.Format(
					"SELECT AlarmId, StateStart, ScheduledId, ScheduledNextId, StateId, ScheduledNextId, NextStart, PlatformTypeId, StateCode, BatchId, OriginalDataSourceId FROM RTA.ActualAgentState WHERE PersonId ='{0}'",
					personIdString);
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
									          : (DateTime?) null,
								OriginalDataSourceId = !reader.IsDBNull(reader.GetOrdinal("OriginalDataSourceId"))
									                       ? reader.GetString(reader.GetOrdinal("OriginalDataSourceId"))
									                       : ""
							};
						LoggingSvc.InfoFormat("Found old state for person: {0}, old statecode: {1}", personToLoad, agentState.StateCode);
						return agentState;
					}
				}
			}
			LoggingSvc.InfoFormat("Found no state for person: {0}", personToLoad);
			return null;
		}

		public IList<IActualAgentState> GetMissingAgentStatesFromBatch(DateTime batchId, string dataSourceId)
		{
			LoggingSvc.InfoFormat("Getting users on DatasourceId: {0}", dataSourceId);

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

		public RtaStateGroupLight AddAndGetNewRtaState(string stateCode, Guid platformTypeId)
		{
			var stateId = Guid.NewGuid();
			var defaultStateGroupId = Guid.Empty;
			var defaultStateGroupName = "";
			var defaultStateGroupBusinessUnit = Guid.Empty;
			const string getDefaultStateGroupQuery = @"SELECT Name, Id, BusinessUnit FROM RtaStateGroup WHERE DefaultStateGroup = 1";
			
			using (var connection = _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.AppConnectionString()))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				command.CommandText = getDefaultStateGroupQuery;
				connection.Open();
				var reader = command.ExecuteReader();
				while (reader.Read())
				{
					defaultStateGroupId = reader.GetGuid(reader.GetOrdinal("Id"));
					defaultStateGroupName = reader.GetString(reader.GetOrdinal("Name"));
					defaultStateGroupBusinessUnit = reader.GetGuid(reader.GetOrdinal("BusinessUnit"));
				}
				reader.Close();
				var addRtaStateQuery = string.Format("INSERT INTO RtaState VALUES ('{0}', '{1}', '{1}', '{2}', '{3}')", stateId,
				                                     stateCode, platformTypeId, defaultStateGroupId);

				command = connection.CreateCommand();
				command.CommandText = addRtaStateQuery;
				command.ExecuteNonQuery();
			}
			return new RtaStateGroupLight
				{
					StateGroupId = defaultStateGroupId,
					StateGroupName = defaultStateGroupName,
					BusinessUnitId = defaultStateGroupBusinessUnit,
					StateName = stateCode,
					PlatformTypeId = platformTypeId,
					StateCode = stateCode,
					StateId = stateId
				};
		}

		public ConcurrentDictionary<string, List<RtaStateGroupLight>> StateGroups()
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
					                         		StateCode = reader.GetString(reader.GetOrdinal("StateCode")),
					                         		StateGroupId = reader.GetGuid(reader.GetOrdinal("StateGroupId")),
					                         		StateId = reader.GetGuid(reader.GetOrdinal("StateId")),
					                         		StateName = reader.GetString(reader.GetOrdinal("StateName")),
													IsLogOutState = reader.GetBoolean(reader.GetOrdinal("IsLogOutState"))
					                         	};
					stateGroups.Add(rtaStateGroupLight);
				}
				reader.Close();
			}
			return new ConcurrentDictionary<string, List<RtaStateGroupLight>>(stateGroups.GroupBy(s => s.StateCode).ToDictionary(g => g.Key, g => g.ToList()));
		}

		public ConcurrentDictionary<Guid, List<RtaAlarmLight>> ActivityAlarms()
		{
			const string query =
				@"SELECT sg.Id StateGroupId , sg.Name StateGroupName, Activity ActivityId, t.Name, t.Id AlarmTypeId,
								t.DisplayColor,t.StaffingEffect, t.ThresholdTime
								FROM StateGroupActivityAlarm a
								INNER JOIN AlarmType t ON a.AlarmType = t.Id
								LEFT JOIN RtaStateGroup sg ON  a.StateGroup = sg.Id 
								WHERE t.IsDeleted = 0
								AND sg.IsDeleted = 0
				UNION ALL								
				SELECT NULL,NULL,Activity ActivityId, t.Name, t.Id AlarmTypeId,
								t.DisplayColor,t.StaffingEffect, t.ThresholdTime
								FROM StateGroupActivityAlarm a
								INNER JOIN AlarmType t ON a.AlarmType = t.Id 
								WHERE t.IsDeleted = 0
								AND a.StateGroup IS NULL";
			var stateGroups = new List<RtaAlarmLight>();
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
							DisplayColor = reader.GetInt32(reader.GetOrdinal("DisplayColor")),
							StaffingEffect = reader.GetDouble(reader.GetOrdinal("StaffingEffect")),
							AlarmTypeId = reader.GetGuid(reader.GetOrdinal("AlarmTypeId")),
							Name = reader.GetString(reader.GetOrdinal("Name")),
							ThresholdTime = reader.GetInt64(reader.GetOrdinal("ThresholdTime"))
						};
					stateGroups.Add(rtaAlarmLight);
				}
				reader.Close();
			}
			return
				new ConcurrentDictionary<Guid, List<RtaAlarmLight>>(stateGroups.GroupBy(g => g.ActivityId)
				                                                               .ToDictionary(k => k.Key, v => v.ToList()));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			MessageId = "0")]
		public void AddOrUpdate(IList<IActualAgentState> actualAgentStates)
		{
			if (!actualAgentStates.Any())
				return;
			
			using (
				var connection =
					_databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.DataStoreConnectionString()))
			{
				connection.Open();
				foreach (var newState in actualAgentStates)
				{
					var command = connection.CreateCommand();
					command.CommandType = CommandType.StoredProcedure;
					command.CommandText = "[RTA].[rta_addorupdate_actualagentstate]";

					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@PersonId",
							SqlDbType = SqlDbType.UniqueIdentifier,
							Direction = ParameterDirection.Input,
							Value = newState.PersonId
						});
					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@StateCode",
							SqlDbType = SqlDbType.NVarChar,
							Direction = ParameterDirection.Input,
							Value = newState.StateCode
						});
					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@PlatformTypeId",
							SqlDbType = SqlDbType.UniqueIdentifier,
							Direction = ParameterDirection.Input,
							Value = newState.PlatformTypeId
						});
					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@State",
							SqlDbType = SqlDbType.NVarChar,
							Direction = ParameterDirection.Input,
							Value = newState.State
						});
					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@AlarmName",
							SqlDbType = SqlDbType.NVarChar,
							Direction = ParameterDirection.Input,
							Value = newState.AlarmName
						});
					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@StateId",
							SqlDbType = SqlDbType.UniqueIdentifier,
							Direction = ParameterDirection.Input,
							Value = newState.StateId
						});
					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@Scheduled",
							SqlDbType = SqlDbType.NVarChar,
							Direction = ParameterDirection.Input,
							Value = newState.Scheduled
						});
					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@ScheduledId",
							SqlDbType = SqlDbType.UniqueIdentifier,
							Direction = ParameterDirection.Input,
							Value = newState.ScheduledId
						});
					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@AlarmId",
							SqlDbType = SqlDbType.UniqueIdentifier,
							Direction = ParameterDirection.Input,
							Value = newState.AlarmId
						});
					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@ScheduledNext",
							SqlDbType = SqlDbType.NVarChar,
							Direction = ParameterDirection.Input,
							Value = newState.ScheduledNext
						});
					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@ScheduledNextId",
							SqlDbType = SqlDbType.UniqueIdentifier,
							Direction = ParameterDirection.Input,
							Value = newState.ScheduledNextId
						});
					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@StateStart",
							SqlDbType = SqlDbType.DateTime,
							Direction = ParameterDirection.Input,
							Value = newState.StateStart
						});
					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@NextStart",
							SqlDbType = SqlDbType.DateTime,
							Direction = ParameterDirection.Input,
							Value = newState.NextStart
						});
					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@AlarmStart",
							SqlDbType = SqlDbType.DateTime,
							Direction = ParameterDirection.Input,
							Value = newState.AlarmStart
						});
					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@Color",
							SqlDbType = SqlDbType.Int,
							Direction = ParameterDirection.Input,
							Value = newState.Color
						});
					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@StaffingEffect",
							SqlDbType = SqlDbType.Float,
							Direction = ParameterDirection.Input,
							Value = newState.StaffingEffect
						});
					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@ReceivedTime",
							SqlDbType = SqlDbType.DateTime,
							Direction = ParameterDirection.Input,
							Value = newState.ReceivedTime
						});
					if (newState.BatchId != null)
					{
						command.Parameters.Add(new SqlParameter
							{
								ParameterName = "@BatchId",
								SqlDbType = SqlDbType.DateTime,
								Direction = ParameterDirection.Input,
								Value = newState.BatchId
							});
					}
					else
						command.Parameters.Add(new SqlParameter
							{
								ParameterName = "@BatchId",
								Direction = ParameterDirection.Input,
								Value = DBNull.Value
							});
					command.Parameters.Add(new SqlParameter
						{
							ParameterName = "@OriginalDataSourceId",
							SqlDbType = SqlDbType.NVarChar,
							Direction = ParameterDirection.Input,
							Value = newState.OriginalDataSourceId
						});
					command.ExecuteNonQuery();
					LoggingSvc.InfoFormat("Saved state: {0} to database", newState);
				}
			}
		}
	}

	public class ScheduleLayer
	{
		public Guid PayloadId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public string Name { get; set; }
		public string ShortName { get; set; }
		public int DisplayColor { get; set; }

		public Color TheColor()
		{
			return Color.FromArgb(DisplayColor);
		}

		public DateTimePeriod Period()
		{
			return new DateTimePeriod(StartDateTime,EndDateTime);
		}
	}
}