namespace Teleopti.Interfaces.Domain
{
	public interface ISeatMap : IAggregateRootWithEvents
	{
		string SeatMapJsonData { get; set; }
	}
}