using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public interface IMaxSeatCallback
	{
		void DatesOptimized(IEnumerable<DateOnly> dates);
	}
}