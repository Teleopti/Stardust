using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatByIndexScore
	{
		public int Index { get; set; }
		public int TotalFrequency { get; set; }
		public int TotalRoleMatches { get; set; }
		public List<TransientSeatBooking> TransientSeatBookingsForASeat { get; set; }
		public int FilledSeatCount { get; set; }
		public int TotalGroupNeighbourCount { get; set; }
		public int HighestPriority { get; set; }
	}
}