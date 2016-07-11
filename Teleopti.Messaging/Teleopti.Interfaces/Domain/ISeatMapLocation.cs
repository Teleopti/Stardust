using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeatMapLocation : IAggregateRootWithEvents
	{

		int SeatCount { get; }
		bool IncludeInSeatPlan { get; set; }
		string SeatMapJsonData { get; set; }
		string Name { get; set; }
		
		IList<ISeat> Seats { get; }
		string LocationPrefix { get; set; }
		string LocationSuffix { get; set; }

		void ClearBookingInformation();

	}
}