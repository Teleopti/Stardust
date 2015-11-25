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

		ISeat GetNextUnallocatedSeat(ISeatBooking booking);
		bool CanAllocateShifts (params ISeatBooking[] agentShifts);

		
		void ClearBookingInformation();

	}
}