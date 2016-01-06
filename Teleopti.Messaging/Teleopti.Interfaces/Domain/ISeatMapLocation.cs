using System;
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

		void ClearBookingInformation();

	}
}