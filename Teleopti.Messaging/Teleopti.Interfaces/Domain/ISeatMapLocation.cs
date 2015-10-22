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

		IList<ISeatMapLocation> ChildLocations { get; }
		ISeatMapLocation ParentLocation { get; set; }
		void ClearBookingInformation();

		ISeatMapLocation GetLocationToAllocateSeats(IEnumerable<ISeatBooking> agentShifts);
		ISeat GetNextUnallocatedSeat(ISeatBooking booking, Boolean ignoreChildren);

	}
}