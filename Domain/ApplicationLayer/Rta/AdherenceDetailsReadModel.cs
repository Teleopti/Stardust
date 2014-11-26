using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AdherenceDetailsReadModel
	{
		public Guid PersonId { get; set; }
		public DateTime Date { get; set; }
		public DateOnly BelongsToDate { get { return new DateOnly(Date); } }
		public AdherenceDetailsModel Model { get; set; }
	}

	public class AdherenceDetailsModel
	{
		public IList<AdherenceDetailModel> DetailModels { get; set; }
		public bool HasActivityEnded { get; set; }
		public bool HasShiftEnded { get; set; }
		public DateTime? ShiftEndTime { get; set; }
		public DateTime? ActualEndTime { get; set; }
		public bool IsInAdherence { get; set; }
	}

	public class AdherenceDetailModel
	{
		public string Name { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? ActualStartTime { get; set; }
		public DateTime? LastStateChangedTime { get; set; }
		public TimeSpan TimeInAdherence { get; set; }
		public TimeSpan TimeOutOfAdherence { get; set; }
	}
}