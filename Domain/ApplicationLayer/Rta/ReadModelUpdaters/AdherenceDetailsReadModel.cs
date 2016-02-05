using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public class AdherenceDetailsReadModel
	{
		public Guid PersonId { get; set; }
		public DateTime Date { get; set; }
		public DateOnly BelongsToDate { get { return new DateOnly(Date); } }

		public int Version { get; set; }

		public AdherenceDetailsModel Model { get; set; }

		public AdherenceDetailsReadModelState State { get; set; }
	}

	public class AdherenceDetailsModel
	{
		public IEnumerable<ActivityAdherence> Activities { get; set; }
		public DateTime? ShiftEndTime { get; set; }
		public DateTime? ActualEndTime { get; set; }
		public bool? LastAdherence { get; set; }
		public DateTime LastUpdate { get; set; }
	}

	public class ActivityAdherence
	{
		public string Name { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime? ActualStartTime { get; set; }
		public TimeSpan? TimeInAdherence { get; set; }
		public TimeSpan? TimeOutOfAdherence { get; set; }
	}

	public class AdherenceDetailsReadModelState
	{
		public AdherenceDetailsReadModelState()
		{
			Activities = new AdherenceDetailsReadModelActivityState[] {};
			Adherence = new AdherenceDetailsReadModelAdherenceState[] { };
		}

		public DateTime? ShiftEndTime { get; set; }
		public DateTime? FirstStateChangeOutOfAdherenceWithLastActivity { get; set; }

		public IEnumerable<AdherenceDetailsReadModelActivityState> Activities { get; set; }
		public IEnumerable<AdherenceDetailsReadModelAdherenceState> Adherence { get; set; }
		public DateTime LastUpdate { get; set; }
	}

	public class AdherenceDetailsReadModelActivityState
	{
		public string Name { get; set; }
		public DateTime StartTime { get; set; }
	}

	public class AdherenceDetailsReadModelAdherenceState
	{
		public DateTime Time { get; set; }
		public AdherenceDetailsReadModelAdherence Adherence { get; set; }
		public bool StateChanged { get; set; }
	}

	public enum AdherenceDetailsReadModelAdherence
	{
		In = 1,
		Out = 2,
		Neutral = 3
	}
}