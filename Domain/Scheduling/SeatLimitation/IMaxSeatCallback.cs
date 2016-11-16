using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public interface IMaxSeatCallback
	{
		void OptimizedDates(IEnumerable<DateOnly> dates);
	}
}