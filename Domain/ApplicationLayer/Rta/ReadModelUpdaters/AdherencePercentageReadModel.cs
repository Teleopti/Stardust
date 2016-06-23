using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public class AdherencePercentageReadModel
	{
		public Guid PersonId { get; set; }
		public DateTime Date { get; set; }
		public DateOnly BelongsToDate { get { return new DateOnly(Date); } }

		public int Version { get; set; }

		public TimeSpan TimeInAdherence { get; set; }
		public TimeSpan TimeOutOfAdherence { get; set; }
		public DateTime? LastTimestamp { get; set; }
		public bool? IsLastTimeInAdherence { get; set; }
		public bool ShiftHasEnded { get; set; }

		public IEnumerable<AdherencePercentageReadModelState> State { get; set; }
	}

	public class AdherencePercentageReadModelState
	{
		public DateTime Timestamp { get; set; }
		public bool? InAdherence { get; set; }
		public bool? ShiftStarted { get; set; }
		public bool? ShiftEnded { get; set; }
	}

}