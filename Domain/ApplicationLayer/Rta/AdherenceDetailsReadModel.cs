using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AdherenceDetailsReadModel
	{
		public Guid PersonId { get; set; }
		public DateOnly Date { get; set; }
		public string Name { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime? ActualStartTime { get; set; }
		public DateTime? LastStateChangedTime { get; set; }
		public bool IsInAdherence { get; set; }
		public TimeSpan TimeInAdherence { get; set; }
		public TimeSpan TimeOutAdherence { get; set; }
	}
}