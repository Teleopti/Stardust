using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling.SeatLimitation
{
	public class DesktopMaxSeatCallback : IMaxSeatCallback
	{
		private readonly ISchedulerStateHolder _stateHolder;

		public DesktopMaxSeatCallback(ISchedulerStateHolder stateHolder)
		{
			_stateHolder = stateHolder;
		}

		public void DatesOptimized(IEnumerable<DateOnly> dates)
		{
			dates.ForEach(x => _stateHolder.MarkDateToBeRecalculated(x));
		}
	}
}