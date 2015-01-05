using System;
using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
	public class ActualAgentState : IActualAgentState
	{
		public ActualAgentState()
		{
			StateCode = "";
			AlarmStart = new DateTime(1900, 1, 1);
			AlarmName = "";
			NextStart = new DateTime(1900, 1, 1);
			ScheduledNext = "";
			StateStart = new DateTime(1900, 1, 1);
			Scheduled = "";
			State = "";
		}

		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public string State { get; set; }
		public Guid StateId { get; set; }
		public string Scheduled { get; set; }
		public DateTime StateStart { get; set; }
		public string ScheduledNext { get; set; }
		public Guid ScheduledNextId { get; set; }
		public DateTime NextStart { get; set; }
		public string AlarmName { get; set; }
		public Guid AlarmId { get; set; }
		public int Color { get; set; }
		public DateTime AlarmStart { get; set; }
		public double StaffingEffect { get; set; }
		public string StateCode { get; set; }
		public Guid ScheduledId { get; set; }
		public Guid PlatformTypeId { get; set; }
		public DateTime ReceivedTime { get; set; }
		public string OriginalDataSourceId { get; set; }
		public DateTime? BatchId { get; set; }

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