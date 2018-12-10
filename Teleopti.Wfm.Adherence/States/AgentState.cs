using System;
using Teleopti.Wfm.Adherence.States.Events;

namespace Teleopti.Wfm.Adherence.States
{
	public class AgentStatePrepare
	{
		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public Guid? TeamId { get; set; }
		public Guid? SiteId { get; set; }
	}

	public class PersonForCheck
	{
		public Guid PersonId { get; set; }
		public DateTime? LastCheck { get; set; }
		public int? LastTimeWindowCheckSum { get; set; }
	}

	public class AgentState
	{
		public DateTime? SnapshotId { get; set; }
		public int? SnapshotDataSourceId { get; set; }

		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public Guid? TeamId { get; set; }
		public Guid? SiteId { get; set; }
		public DateTime? ReceivedTime { get; set; }
		public Guid? StateGroupId { get; set; }
		public DateTime? StateStartTime { get; set; }

		public Guid? ActivityId { get; set; }

		public Guid? RuleId { get; set; }
		public DateTime? RuleStartTime { get; set; }
		public EventAdherence? Adherence { get; set; }

		public DateTime? AlarmStartTime { get; set; }

		public int? TimeWindowCheckSum { get; set; }
	}

	public static class AgentStateExtensions
	{
		public static DateTime ReceivedTime(this AgentState stored)
		{
			return stored?.ReceivedTime ?? DateTime.MinValue;
		}
	}
}
