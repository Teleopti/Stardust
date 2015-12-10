using System;
using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
	public class AgentStateReadModel
	{
		public Guid PlatformTypeId { get; set; }
		public string OriginalDataSourceId { get; set; }
		public DateTime? BatchId { get; set; }

		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public Guid? TeamId { get; set; }
		public Guid? SiteId { get; set; }
		public DateTime ReceivedTime { get; set; }

		public string StateCode { get; set; }
		public string State { get; set; }
		public Guid? StateId { get; set; }
		public DateTime? StateStartTime { get; set; }

		public Guid? ScheduledId { get; set; }
		public string Scheduled { get; set; }
		public Guid? ScheduledNextId { get; set; }
		public string ScheduledNext { get; set; }
		public DateTime? NextStart { get; set; }
		
		public Guid? AlarmId { get; set; }
		public DateTime? AdherenceStartTime { get; set; }
		public string AlarmName { get; set; }
		public int? Color { get; set; }
		public double? StaffingEffect { get; set; }
		public int? Adherence { get; set; }

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture,
				"PersonId: {0}, StateCode: {1} StateGroup: {2}, Scheduled: {3}, StateStartTime: {4}, Scheduled next: {5}, NextStart: {6}, Alarm: {7}, AdherenceStartTime: {8}, BatchId: {9}",
				PersonId, StateCode, State, Scheduled, StateStartTime, ScheduledNext, NextStart, AlarmName, AdherenceStartTime, BatchId);

		}

	}
}