using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentState
	{
		public DateTime? BatchId { get; set; }
		public Guid PlatformTypeId { get; set; }
		public string SourceId { get; set; }

		public Guid PersonId { get; set; }
		public Guid BusinessUnitId { get; set; }
		public Guid? TeamId { get; set; }
		public Guid? SiteId { get; set; }
		public DateTime ReceivedTime { get; set; }
		public string StateCode { get; set; }
		public Guid? StateGroupId { get; set; }
		public DateTime? StateStartTime { get; set; }

		public Guid? ActivityId { get; set; }
		public Guid? NextActivityId { get; set; }
		public DateTime? NextActivityStartTime { get; set; }

		public Guid? RuleId { get; set; }
		public DateTime? RuleStartTime { get; set; }
		public double? StaffingEffect { get; set; }
		public Adherence? Adherence { get; set; }

		public DateTime? AlarmStartTime { get; set; }

		public int? TimeWindowCheckSum { get; set; }
	}

	public static class AgentStateExtensions
	{
		public static Guid PlatformTypeId(this AgentState stored)
		{
			return stored.get(s => s.PlatformTypeId);
		}

		public static string SourceId(this AgentState stored)
		{
			return stored.get(s => s.SourceId);
		}

		public static DateTime ReceivedTime(this AgentState stored)
		{
			return stored.get(s => s.ReceivedTime);
		}
		


		private static T get<T>(this AgentState stored, Func<AgentState, T> getter)
		{
			if (stored == null)
				return default(T);
			return getter(stored);
		}
	}
}
