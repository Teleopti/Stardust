using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatScore
	{
		public ISeatBooking SeatBooking { get; set; }
		public ISeat Seat { get; set; }
		public ISeatMapLocation Location { get; set; }
		public int RoleMatches { get; set; }
		public int Frequency { get; set; }
	}
}