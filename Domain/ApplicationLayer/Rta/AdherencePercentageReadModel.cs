using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AdherencePercentageReadModel
	{
		public AdherencePercentageReadModel()
		{
			Saga = new List<AdherencePercentageState>();
		}
		public Guid PersonId { get; set; }
		/// <summary>
		/// A wrapper to handle the transformation for NHib
		/// </summary>
		public DateTime Date { get; set; }
		public DateOnly BelongsToDate { get { return new DateOnly(Date); } }
		public TimeSpan TimeInAdherence { get; set; }
		public TimeSpan TimeOutOfAdherence { get; set; }

		public DateTime? LastTimestamp { get; set; }
		public bool? IsLastTimeInAdherence { get; set; }
		public bool ShiftHasEnded { get; set; }
		public List<AdherencePercentageState> Saga { get; set; }
	}
}