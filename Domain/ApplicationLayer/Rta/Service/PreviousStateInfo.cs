using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class PreviousStateInfo
	{
		public PreviousStateInfo(Guid personId)
		{
			PersonId = personId;
			StateGroupId = Guid.NewGuid();
		}

		public PreviousStateInfo(Guid personId, AgentStateReadModel fromStorage)
		{
			PersonId = personId;
			StateGroupId = Guid.NewGuid();
			if (fromStorage == null)
				return;
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
			AlarmTypeId = fromStorage.AlarmId;
			AlarmTypeStartTime = fromStorage.StateStart;
			StaffingEffect = fromStorage.StaffingEffect;
			Adherence = (AdherenceState?)fromStorage.Adherence;
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

		public Guid? AlarmTypeId { get; set; }
		public DateTime? AlarmTypeStartTime { get; set; }
		public double? StaffingEffect { get; set; }
		public AdherenceState? Adherence { get; set; }
	}
}
