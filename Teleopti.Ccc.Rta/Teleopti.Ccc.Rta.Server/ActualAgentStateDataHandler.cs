using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server
{
	public interface IActualAgentStateDataHandler
	{
		IList<ScheduleLayer> CurrentLayerAndNext(DateTime onTime, Guid personId);
		IActualAgentState LoadOldState(Guid value);
		IEnumerable<RtaStateGroupLight> StateGroups();
		IEnumerable<RtaAlarmLight> ActivityAlarms();
		void AddOrUpdate(ActualAgentState newState);
		ICollection<Guid> PersonIdsWithExternalLogon(Guid businessUnitId);
	}

	public class ActualAgentStateDataHandler : IActualAgentStateDataHandler
	{
		private readonly IDatabaseConnectionFactory _databaseConnectionFactory;
		private readonly IDatabaseConnectionStringHandler _databaseConnectionStringHandler;
		
		public ActualAgentStateDataHandler(IDatabaseConnectionFactory databaseConnectionFactory,  IDatabaseConnectionStringHandler databaseConnectionStringHandler)
		{
			_databaseConnectionFactory = databaseConnectionFactory;
			_databaseConnectionStringHandler = databaseConnectionStringHandler;
			
		}

		public IList<ScheduleLayer> CurrentLayerAndNext(DateTime onTime, Guid personId)
		{
			//using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
			//{
			//    var layers = _scheduleProjectionReadOnlyRepository.CurrentLayerAndNext(onTime, personId);

			//    var scheduleLayer = layers.FirstOrDefault();
			//    ScheduleLayer nextLayer = null;

			//    // no layer now
			//    if (scheduleLayer != null && scheduleLayer.StartDateTime > onTime)
			//    {
			//        nextLayer = scheduleLayer;
			//        scheduleLayer = null;
			//    }

			//    if (nextLayer == null && layers != null && layers.Count > 1)
			//    {
			//        nextLayer = layers[1];
			//    }

			//    if (scheduleLayer != null && nextLayer != null)
			//    {
			//        //scheduleLayer is the last in assignment
			//        if (scheduleLayer.EndDateTime != nextLayer.StartDateTime)
			//        {
			//            nextLayer = null;
			//        }
			//    }
			//    return new List<ScheduleLayer> { scheduleLayer, nextLayer };
			//}
			return new List<ScheduleLayer>();
		}


		public IActualAgentState LoadOldState(Guid value)
		{
			return null; // _statisticRepository.LoadOneActualAgentState(value);
		}

		public IEnumerable<RtaStateGroupLight> StateGroups()
		{
			const string query = @"SELECT s.Id StateId, s.Name StateName, s.PlatformTypeId,s.StateCode, sg.Id StateGroupId , BusinessUnit BusinessUnitId  
								FROM RtaStateGroup sg
								INNER JOIN RtaState s ON s.Parent = sg.Id 
								ORDER BY StateCode";
			var stateGroups = new List<RtaStateGroupLight>();
			using (var connection = _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.AppConnectionString()))
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

		public IEnumerable<RtaAlarmLight> ActivityAlarms()
		{
			const string query = @"SELECT sg.Id StateGroupId , sg.Name StateGroupName, Activity ActivityId, t.Name, t.Id AlarmTypeId,
								t.DisplayColor,t.StaffingEffect, t.ThresholdTime--, sg.BusinessUnit BusinessUnitId
								FROM StateGroupActivityAlarm a
								INNER JOIN AlarmType t ON a.AlarmType = t.Id
								INNER JOIN RtaStateGroup sg ON  a.StateGroup = sg.Id
								ORDER BY Activity";
			var stateGroups = new List<RtaAlarmLight>();
			using (var connection = _databaseConnectionFactory.CreateConnection(_databaseConnectionStringHandler.AppConnectionString()))
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
					var activityId = reader.GetGuid(reader.GetOrdinal("ActivityId"));
					var thresholdTime = reader.GetInt64(reader.GetOrdinal("StaffingEffect"));
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
			return stateGroups;

		}

		public void AddOrUpdate(ActualAgentState newState)
		{
			//_statisticRepository.AddOrUpdateActualAgentState(newState);
		}

		public ICollection<Guid> PersonIdsWithExternalLogon(Guid businessUnitId)
		{
			return null;
			//return _statisticRepository.PersonIdsWithExternalLogon(Guid.NewGuid());
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