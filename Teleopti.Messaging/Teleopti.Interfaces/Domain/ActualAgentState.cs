﻿using System;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// 
	/// </summary>
	public class ActualAgentState : IActualAgentState
	{
		private DateTime _stateStart = new DateTime(1900, 1, 1);
		private DateTime _nextStart = new DateTime(1900, 1, 1);
		private DateTime _alarmStart = new DateTime(1900, 1, 1);
		private string _stateCode = "";
		private string _state = "";
		private string _scheduled = "";
		private string _scheduledNext = "";
		private string _alarmName = "";
		
		/// <summary>
		/// 
		/// </summary>
		public Guid PersonId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string State
		{
			get { return _state; }
			set { _state = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Guid StateId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string Scheduled
		{
			get { return _scheduled; }
			set { _scheduled = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public DateTime StateStart
		{
			get { return _stateStart; }
			set { _stateStart = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string ScheduledNext
		{
			get { return _scheduledNext; }
			set { _scheduledNext = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Guid ScheduledNextId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime NextStart
		{
			get { return _nextStart; }
			set { _nextStart = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string AlarmName
		{
			get { return _alarmName; }
			set { _alarmName = value; }
		}
		
		/// <summary>
		/// 
		/// </summary>
		public Guid AlarmId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public int Color { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public DateTime AlarmStart
		{
			get { return _alarmStart; }
			set { _alarmStart = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public double StaffingEffect { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public string StateCode
		{
			get { return _stateCode; }
			set { _stateCode = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public Guid ScheduledId { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Guid PlatformTypeId { get; set; }

		public DateTime Timestamp { get; set; }

		public TimeSpan TimeInState { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool Equals(IActualAgentState other)
		{
			return GetHashCode().Equals(other.GetHashCode());
		}

		/// <summary>
		/// 
		/// </summary>
		public override int GetHashCode()
		{
			unchecked{
            var result = 0;
        	result = (result*397) ^ ScheduledId.GetHashCode();
        	result = (result*397) ^ ScheduledNextId.GetHashCode();
        	result = (result*397) ^ AlarmId.GetHashCode();
			result = (result * 397) ^ StateId.GetHashCode();
			result = (result * 397) ^ ScheduledNext.GetHashCode();
			result = (result * 397) ^ NextStart.GetHashCode();
            return result;
			}
		}
	}
}