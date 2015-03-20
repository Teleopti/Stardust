using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeatMapLocation : IAggregateRootWithEvents
	{

		string SeatMapJsonData { get; set; }
		IList<ISeat> Seats { get; }
	}
}