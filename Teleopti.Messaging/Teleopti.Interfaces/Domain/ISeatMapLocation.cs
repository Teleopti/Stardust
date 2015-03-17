namespace Teleopti.Interfaces.Domain
{
	public interface ISeatMapLocation : IAggregateRootWithEvents
	{

		string SeatMapJsonData { get; set; }
	}

	
}