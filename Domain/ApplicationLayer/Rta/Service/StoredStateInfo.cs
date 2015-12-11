using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StoredStateInfo
	{
		public StoredStateInfo(Guid personId)
		{
			PersonId = personId;
		}

		public StoredStateInfo(Guid personId, AgentStateReadModel fromStorage)
		{
			PersonId = personId;
			PersonId = fromStorage.PersonId;
			BatchId = fromStorage.BatchId;
			PlatformTypeId = fromStorage.PlatformTypeId;
			SourceId = fromStorage.OriginalDataSourceId;
			ReceivedTime = fromStorage.ReceivedTime;
			StateCode = fromStorage.StateCode;
			StateGroupId = fromStorage.StateId;
			ActivityId = fromStorage.ScheduledId;
			NextActivityId = fromStorage.ScheduledNextId;
			NextActivityStartTime = fromStorage.NextStart;
			RuleId = fromStorage.AlarmId;
			AlarmStartTime = fromStorage.AlarmStartTime;
			AdherenceStartTime = fromStorage.AdherenceStartTime;
			StateStartTime = fromStorage.StateStartTime;
			StaffingEffect = fromStorage.StaffingEffect;
			Adherence = (Adherence?)fromStorage.Adherence;
		}

		public DateTime? BatchId { get; set; }
		public Guid PlatformTypeId { get; set; }
		public string SourceId { get; set; }

		public Guid PersonId { get; set; }
		public DateTime ReceivedTime { get; set; }
		public string StateCode { get; set; }
		public Guid? StateGroupId { get; set; }
		public Guid? ActivityId { get; set; }

		public Guid? NextActivityId { get; set; }
		public DateTime? NextActivityStartTime { get; set; }

		public Guid? RuleId { get; set; }
		public DateTime? AdherenceStartTime { get; set; }
		public DateTime? StateStartTime { get; set; }
		public double? StaffingEffect { get; set; }
		public Adherence? Adherence { get; set; }
		public DateTime? AlarmStartTime { get; set; }
	}

	public static class StoredExtensions
	{
		public static Guid PlatformTypeId(this StoredStateInfo stored)
		{
			return stored.get(s => s.PlatformTypeId);
		}

		public static Guid? ActivityId(this StoredStateInfo stored)
		{
			return stored.get(s => s.ActivityId);
		}

		public static string StateCode(this StoredStateInfo stored)
		{
			return stored.get(s => s.StateCode);
		}

		public static DateTime? BatchId(this StoredStateInfo stored)
		{
			return stored.get(s => s.BatchId);
		}

		public static Guid? RuleId(this StoredStateInfo stored)
		{
			return stored.get(s => s.RuleId);
		}

		public static DateTime ReceivedTime(this StoredStateInfo stored)
		{
			return stored.get(s => s.ReceivedTime);
		}

		public static DateTime? NextActivityStartTime(this StoredStateInfo stored)
		{
			return stored.get(s => s.NextActivityStartTime);
		}
		public static string SourceId(this StoredStateInfo stored)
		{
			return stored.get(s => s.SourceId);
		}
		

		public static Guid? NextActivityId(this StoredStateInfo stored)
		{
			return stored.get(s => s.NextActivityId);
		}
		
		public static Guid? StateGroupId(this StoredStateInfo stored)
		{
			return stored.get(s => s.StateGroupId);
		}

		

		private static T get<T>(this StoredStateInfo stored, Func<StoredStateInfo, T> getter)
		{
			if (stored == null)
				return default(T);
			return getter(stored);
		}
	}
}
