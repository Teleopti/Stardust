namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ITeamGroupedBooking
	{
		ITeam Team { get; }
		ISeatBooking SeatBooking { get; }
	}
}