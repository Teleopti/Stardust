namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISeatOccupancyFrequency
	{
		ISeat Seat { get; set; }
		int Frequency { get; set; }

	}
}