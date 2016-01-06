namespace Teleopti.Interfaces.Domain
{
	public interface ISeatOccupancyFrequency
	{
		ISeat Seat { get; set; }
		int Frequency { get; set; }

	}
}