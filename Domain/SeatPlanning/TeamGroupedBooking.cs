using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class TeamGroupedBooking : ITeamGroupedBooking
	{
		public ITeam Team { get; private set; }
		public ISeatBooking SeatBooking { get; private set; }

		public TeamGroupedBooking(ITeam team, ISeatBooking seatBooking)
		{
			Team = team;
			SeatBooking = seatBooking;
		}
	}
}