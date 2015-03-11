using System;
using System.Globalization;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;

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
		public DateTime? StateStart { get; set; }

		public Guid? ScheduledId { get; set; }
		public string Scheduled { get; set; }
		public Guid? ScheduledNextId { get; set; }
		public string ScheduledNext { get; set; }
		public DateTime? NextStart { get; set; }
		
		public Guid? AlarmId { get; set; }
		public DateTime? AlarmStart { get; set; }
		public string AlarmName { get; set; }
		public int? Color { get; set; }
		public double? StaffingEffect { get; set; }
		public AdherenceState? Adherence { get; set; }

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