using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AdherencePercentageReadModel
	{
		public Guid PersonId { get; set; }
		/// <summary>
		/// A wrapper to handle the transformation for NHib
		/// </summary>
		public DateTime Date { get; set; }
		public DateOnly BelongsToDate { get { return new DateOnly(Date); } }
		public DateTime LastTimestamp { get; set; }
		public bool IsLastTimeInAdherence { get; set; }
		public DateTime? ShiftEnd { get; set; }
		public TimeSpan TimeInAdherence { get; set; }
		public TimeSpan TimeOutOfAdherence { get; set; }
	}
}