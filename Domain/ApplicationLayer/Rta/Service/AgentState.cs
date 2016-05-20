using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentState
	{
		public AgentState()
		{
		}

		public AgentState(AgentStateReadModel fromStorage)
		{
			PersonId = fromStorage.PersonId;
			BusinessUnitId = fromStorage.BusinessUnitId;
			TeamId = fromStorage.TeamId;
			SiteId = fromStorage.SiteId;
			BatchId = fromStorage.BatchId;
			PlatformTypeId = fromStorage.PlatformTypeId;
			SourceId = fromStorage.OriginalDataSourceId;
			ReceivedTime = fromStorage.ReceivedTime;

			StateCode = fromStorage.StateCode;
			StateGroupId = fromStorage.StateId;
			StateStartTime = fromStorage.StateStartTime;

			ActivityId = fromStorage.ScheduledId;
			NextActivityId = fromStorage.ScheduledNextId;
			NextActivityStartTime = fromStorage.NextStart;

			RuleId = fromStorage.RuleId;
			RuleStartTime = fromStorage.RuleStartTime;
			StaffingEffect = fromStorage.StaffingEffect;
			Adherence = (Adherence?)fromStorage.Adherence;

			AlarmStartTime = fromStorage.AlarmStartTime;
		}

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
	}

	public static class AgentStateExtensions
	{
		public static DateTime? BatchId(this AgentState stored)
		{
			return stored.get(s => s.BatchId);
		}

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



		public static string StateCode(this AgentState stored)
		{
			return stored.get(s => s.StateCode);
		}

		public static Guid? StateGroupId(this AgentState stored)
		{
			return stored.get(s => s.StateGroupId);
		}



		public static Guid? ActivityId(this AgentState stored)
		{
			return stored.get(s => s.ActivityId);
		}

		public static DateTime? NextActivityStartTime(this AgentState stored)
		{
			return stored.get(s => s.NextActivityStartTime);
		}

		public static Guid? NextActivityId(this AgentState stored)
		{
			return stored.get(s => s.NextActivityId);
		}



		public static Guid? RuleId(this AgentState stored)
		{
			return stored.get(s => s.RuleId);
		}

		

		private static T get<T>(this AgentState stored, Func<AgentState, T> getter)
		{
			if (stored == null)
				return default(T);
			return getter(stored);
		}
	}
}
