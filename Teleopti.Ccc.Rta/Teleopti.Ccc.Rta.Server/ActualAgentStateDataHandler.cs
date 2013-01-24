using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server
{
	public interface IActualAgentStateDataHandler
	{
		IList<ScheduleLayer> CurrentLayerAndNext(DateTime onTime, Guid personId);
		IActualAgentState LoadOldState(Guid personToLoad);
		IEnumerable<RtaStateGroupLight> StateGroups();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		Dictionary<Guid, List<RtaAlarmLight>> ActivityAlarms();
		void AddOrUpdate(IActualAgentState newState);
	}

	public class ActualAgentStateDataHandler : IActualAgentStateDataHandler
	{
		private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
		private readonly IDatabaseConnectionStringHandler _databaseConnectionStringHandler;

		public ActualAgentStateDataHandler(IDatabaseConnectionFactory databaseConnectionFactory,
		                                   IDatabaseConnectionStringHandler databaseConnectionStringHandler)
		{
			_databaseConnectionFactory = databaseConnectionFactory;
			_databaseConnectionStringHandler = databaseConnectionStringHandler;

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
			"CA2100:Review SQL queries for security vulnerabilities")]
		public IList<ScheduleLayer> CurrentLayerAndNext(DateTime onTime, Guid personId)
		{
			var dateString = string.Format(CultureInfo.InvariantCulture, onTime.ToString("yyyy-MM-dd HH:mm"));
			var idString = string.Format(CultureInfo.InvariantCulture, personId.ToString());
			var query = string.Format(CultureInfo.InvariantCulture,
			                          @"SELECT TOP 2 PayloadId,StartDateTime,EndDateTime,rta.Name,rta.ShortName,DisplayColor 
						FROM ReadModel.v_ScheduleProjectionReadOnlyRTA rta
						WHERE EndDateTime > '{0}' 
						AND PersonId='{1}'",
			                          dateString, idString);
			var layers = new List<ScheduleLayer>();
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
					var payloadId = reader.GetGuid(reader.GetOrdinal("PayloadId"));
					var startDateTime = reader.GetDateTime(reader.GetOrdinal("StartDateTime"));
					var endDateTime = reader.GetDateTime(reader.GetOrdinal("EndDateTime"));
					var name = reader.GetString(reader.GetOrdinal("Name"));
					var shortName = reader.GetString(reader.GetOrdinal("ShortName"));
					var displayColor = reader.GetInt32(reader.GetOrdinal("DisplayColor"));

					var layer = new ScheduleLayer
					            	{
					            		PayloadId = payloadId,
					            		StartDateTime = startDateTime,
					            		EndDateTime = endDateTime,
					            		Name = name,
					            		ShortName = shortName,
					            		DisplayColor = displayColor
					            	};
					layers.Add(layer);
				}
				reader.Close();
			}

			var scheduleLayer = layers.FirstOrDefault();
			ScheduleLayer nextLayer = null;

			// no layer now
			if (scheduleLayer != null && scheduleLayer.StartDateTime > onTime)
			{
				nextLayer = scheduleLayer;
				scheduleLayer = null;
			}

			if (nextLayer == null && layers.Count > 1)
				nextLayer = layers[1];

			if (scheduleLayer != null && nextLayer != null)
				//scheduleLayer is the last in assignment
				if (scheduleLayer.EndDateTime != nextLayer.StartDateTime)
					nextLayer = null;

			return new List<ScheduleLayer> {scheduleLayer, nextLayer};

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
			"CA2100:Review SQL queries for security vulnerabilities")]
		public IActualAgentState LoadOldState(Guid personToLoad)
		{
			var personIdString = personToLoad.ToString();
			var query = string.Format("SELECT * FROM RTA.ActualAgentState WHERE PersonId ='{0}'", personIdString);
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
						var stateCode = reader.GetString(reader.GetOrdinal("StateCode"));
						var state = reader.GetString(reader.GetOrdinal("State"));
						var alarmName = reader.GetString(reader.GetOrdinal("AlarmName"));
						var scheduled = reader.GetString(reader.GetOrdinal("Scheduled"));
						var scheduledNext = reader.GetString(reader.GetOrdinal("ScheduledNext"));

						var platformTypeId = reader.GetGuid(reader.GetOrdinal("PlatformTypeId"));
						var scheduledId = reader.GetGuid(reader.GetOrdinal("ScheduledId"));
						var alarmId = reader.GetGuid(reader.GetOrdinal("AlarmId"));
						var scheduledNextId = reader.GetGuid(reader.GetOrdinal("ScheduledNextId"));
						var personId = reader.GetGuid(reader.GetOrdinal("PersonId"));
						var stateId = reader.GetGuid(reader.GetOrdinal("StateId"));

						var stateStart = reader.GetDateTime(reader.GetOrdinal("StateStart"));
						var nextStart = reader.GetDateTime(reader.GetOrdinal("NextStart"));
						var alarmStart = reader.GetDateTime(reader.GetOrdinal("AlarmStart"));

						var color = reader.GetInt32(reader.GetOrdinal("Color"));
						var staffingEffect = reader.GetDouble(reader.GetOrdinal("StaffingEffect"));

						return new ActualAgentState
						       	{
						       		State = state,
						       		PlatformTypeId = platformTypeId,
						       		StateCode = stateCode,
						       		AlarmName = alarmName,
						       		StateId = stateId,
						       		Scheduled = scheduled,
						       		ScheduledNext = scheduledNext,
						       		ScheduledId = scheduledId,
						       		AlarmId = alarmId,
						       		ScheduledNextId = scheduledNextId,
						       		PersonId = personId,
						       		StateStart = stateStart,
						       		NextStart = nextStart,
						       		AlarmStart = alarmStart,
						       		Color = color,
						       		StaffingEffect = staffingEffect
						       	};

					}

				}
			}
			return null;
		}

		public IEnumerable<RtaStateGroupLight> StateGroups()
		{
			const string query =
				@"SELECT s.Id StateId, s.Name StateName, s.PlatformTypeId,s.StateCode, sg.Id StateGroupId , BusinessUnit BusinessUnitId  
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
					var stateGroupId = reader.GetGuid(reader.GetOrdinal("StateGroupId"));
					var stateId = reader.GetGuid(reader.GetOrdinal("StateId"));
					var stateCode = reader.GetString(reader.GetOrdinal("StateCode"));
					var businessUnitId = reader.GetGuid(reader.GetOrdinal("BusinessUnitId"));
					var platformTypeId = reader.GetGuid(reader.GetOrdinal("PlatformTypeId"));

					var rtaStateGroupLight = new RtaStateGroupLight
					                         	{
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
			return stateGroups;
		}

		public Dictionary<Guid, List<RtaAlarmLight>> ActivityAlarms()
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
			return stateGroups.GroupBy(g => g.ActivityId).ToDictionary(k => k.Key, v => v.ToList());

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			MessageId = "0")]
		public void AddOrUpdate(IActualAgentState newState)
		{
			using (
				var connection =
					_databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.DataStoreConnectionString()))
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
				connection.Open();
				command.ExecuteNonQuery();
			}
		}
	}

	public class RtaStateGroupLight
	{
		public Guid StateId { get; set; }
		public string StateName { get; set; }
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