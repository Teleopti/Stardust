namespace Teleopti.Interfaces.Domain
{
	public interface ITeamGroupedBooking
	{
		ITeam Team { get; }
		ISeatBooking SeatBooking { get; }
	}
}