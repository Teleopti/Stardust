using System;
using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
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
		
		public Guid PersonId { get; set; }
		public string State
		{
			get { return _state; }
			set { _state = value; }
		}
		public Guid StateId { get; set; }
		public string Scheduled
		{
			get { return _scheduled; }
			set { _scheduled = value; }
		}
		public DateTime StateStart
		{
			get { return _stateStart; }
			set { _stateStart = value; }
		}
		public string ScheduledNext
		{
			get { return _scheduledNext; }
			set { _scheduledNext = value; }
		}
		public Guid ScheduledNextId { get; set; }
		public DateTime NextStart
		{
			get { return _nextStart; }
			set { _nextStart = value; }
		}
		public string AlarmName
		{
			get { return _alarmName; }
			set { _alarmName = value; }
		}
		public Guid AlarmId { get; set; }
		public int Color { get; set; }
		public DateTime AlarmStart
		{
			get { return _alarmStart; }
			set { _alarmStart = value; }
		}
		public double StaffingEffect { get; set; }
		public string StateCode
		{
			get { return _stateCode; }
			set { _stateCode = value; }
		}
		public Guid ScheduledId { get; set; }
		public Guid PlatformTypeId { get; set; }
		public DateTime ReceivedTime { get; set; }
		public string OriginalDataSourceId { get; set; }
		public DateTime? BatchId { get; set; }

		public Guid BusinessUnit { get; set; }

		public bool InAdherence { get { return StaffingEffect.Equals(0); } }

		public override string ToString()
		{
			return BatchId.HasValue
				       ? string.Format(CultureInfo.InvariantCulture,
				                       "PersonId: {0}, State: {1}, Scheduled: {2}, StateStart: {3}, Scheduled next: {4}, NextStart: {5}, Alarm: {6}, AlarmStart: {7}, BatchId: {8}",
				                       PersonId, State, Scheduled, StateStart, ScheduledNext, NextStart, AlarmName, AlarmStart,
				                       BatchId)
				       : string.Format(CultureInfo.InvariantCulture,
				                       "PersonId: {0}, State: {1}, Scheduled: {2}, StateStart: {3}, Scheduled next: {4}, NextStart: {5}, Alarm: {6}, AlarmStart: {7}",
				                       PersonId, State, Scheduled, StateStart, ScheduledNext, NextStart, AlarmName, AlarmStart);
		}

	}
}