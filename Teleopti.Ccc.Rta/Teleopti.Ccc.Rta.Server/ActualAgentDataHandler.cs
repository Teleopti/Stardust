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
	public interface IActualAgentDataHandler
	{
		IList<ScheduleLayer> CurrentLayerAndNext(DateTime onTime, IList<ScheduleLayer> layers);
		IActualAgentState LoadOldState(Guid personToLoad);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		ConcurrentDictionary<string, List<RtaStateGroupLight>> StateGroups();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		ConcurrentDictionary<Guid, List<RtaAlarmLight>> ActivityAlarms();
		void AddOrUpdate(IEnumerable<IActualAgentState> states);
		IList<ScheduleLayer> GetReadModel(Guid personId);
	}

	public class ActualAgentDataHandler : IActualAgentDataHandler
	{
		private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
		private readonly IDatabaseConnectionStringHandler _databaseConnectionStringHandler;
		private static readonly ILog LoggingSvc = LogManager.GetLogger(typeof(IActualAgentDataHandler));

		public ActualAgentDataHandler(IDatabaseConnectionFactory databaseConnectionFactory,
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
					"SELECT AlarmId, StateStart, ScheduledId, ScheduledNextId, StateId, ScheduledNextId, NextStart, PlatformTypeId, StateCode FROM RTA.ActualAgentState WHERE PersonId ='{0}'",
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
								NextStart = reader.GetDateTime(reader.GetOrdinal("NextStart"))
							};
						LoggingSvc.InfoFormat("Found old state for person: {0}, old statecode: {1}", personToLoad, agentState.StateCode);
						return agentState;
					}
				}
			}
			LoggingSvc.InfoFormat("Found no state for person: {0}", personToLoad);
			return null;
		}

		public ConcurrentDictionary<string, List<RtaStateGroupLight>> StateGroups()
		{
			const string query =
				@"SELECT s.Id StateId, s.Name StateName, s.PlatformTypeId,s.StateCode, sg.Id StateGroupId, sg.Name StateGroupName, BusinessUnit BusinessUnitId  
								FROM RtaStateGroup sg
								INNER JOIN RtaState s ON s.Parent = sg.Id 
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
					var stateName = reader.GetString(reader.GetOrdinal("StateName"));
					var stateGroupName = reader.GetString(reader.GetOrdinal("StateGroupName"));
					var stateGroupId = reader.GetGuid(reader.GetOrdinal("StateGroupId"));
					var stateId = reader.GetGuid(reader.GetOrdinal("StateId"));
					var stateCode = reader.GetString(reader.GetOrdinal("StateCode"));
					var businessUnitId = reader.GetGuid(reader.GetOrdinal("BusinessUnitId"));
					var platformTypeId = reader.GetGuid(reader.GetOrdinal("PlatformTypeId"));

					var rtaStateGroupLight = new RtaStateGroupLight
					                         	{
													StateGroupName = stateGroupName,
					                         		BusinessUnitId = businessUnitId,
					                         		PlatformTypeId = platformTypeId,
					                         		StateCode = stateCode,
					                         		StateGroupId = stateGroupId,
					                         		StateId = stateId,
					                         		StateName = stateName
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
								t.DisplayColor,t.StaffingEffect, t.ThresholdTime--, sg.BusinessUnit BusinessUnitId
								FROM StateGroupActivityAlarm a
								INNER JOIN AlarmType t ON a.AlarmType = t.Id
								INNER JOIN RtaStateGroup sg ON  a.StateGroup = sg.Id
								ORDER BY Activity";
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
					var stateGroupName = reader.GetString(reader.GetOrdinal("StateGroupName"));
					var stateGroupId = reader.GetGuid(reader.GetOrdinal("StateGroupId"));
					var alarmTypeId = reader.GetGuid(reader.GetOrdinal("AlarmTypeId"));
					var staffingEffect = reader.GetDouble(reader.GetOrdinal("StaffingEffect"));
					var displayColor = reader.GetInt32(reader.GetOrdinal("DisplayColor"));
					var activityId = Guid.Empty;
					if (!reader.IsDBNull(reader.GetOrdinal("ActivityId")))
						activityId = reader.GetGuid(reader.GetOrdinal("ActivityId"));
					var thresholdTime = reader.GetInt64(reader.GetOrdinal("ThresholdTime"));
					var name = reader.GetString(reader.GetOrdinal("Name"));

					var rtaAlarmLight = new RtaAlarmLight
					                    	{
					                    		DisplayColor = displayColor,
					                    		ActivityId = activityId,
					                    		StaffingEffect = staffingEffect,
					                    		StateGroupId = stateGroupId,
					                    		AlarmTypeId = alarmTypeId,
					                    		StateGroupName = stateGroupName,
					                    		ThresholdTime = thresholdTime,
					                    		Name = name
					                    	};
					stateGroups.Add(rtaAlarmLight);
				}
				reader.Close();
			}
			return new ConcurrentDictionary<Guid, List<RtaAlarmLight>>(stateGroups.GroupBy(g => g.ActivityId).ToDictionary(k => k.Key, v => v.ToList()));

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			MessageId = "0")]
		public void AddOrUpdate(IEnumerable<IActualAgentState> states)
		{
			using (
				var connection =
					_databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.DataStoreConnectionString()))
			{
				connection.Open();
				foreach (var newState in states)
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
					command.ExecuteNonQuery();
					LoggingSvc.InfoFormat("Saved state: {0} to database", newState);
				}
			}
		}
	}

	public class RtaStateGroupLight
	{
		public Guid StateId { get; set; }
		public string StateName { get; set; }
		public string StateGroupName { get; set; }
		public Guid PlatformTypeId { get; set; }
		public string StateCode { get; set; }
		public Guid StateGroupId { get; set; }
		public Guid BusinessUnitId { get; set; }
	}
	
	public class RtaAlarmLight
	{
		public Guid StateGroupId { get; set; }
		public string StateGroupName { get; set; }
		public Guid ActivityId { get; set; }
		public string Name { get; set; }
		public Guid AlarmTypeId { get; set; }
		public int DisplayColor { get; set; }
		public double StaffingEffect { get; set; }
		public long ThresholdTime { get; set; }
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