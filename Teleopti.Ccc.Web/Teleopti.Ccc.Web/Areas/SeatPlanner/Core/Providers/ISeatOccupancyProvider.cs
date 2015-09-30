using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public interface ISeatOccupancyProvider
	{
		IList<GroupedOccupancyViewModel> Get (IList<Guid> seatIds, DateOnly date);
		IList<OccupancyViewModel> GetSeatBookingsForScheduleDays (List<IScheduleDay> dateOnlyPeriod);
	}
}