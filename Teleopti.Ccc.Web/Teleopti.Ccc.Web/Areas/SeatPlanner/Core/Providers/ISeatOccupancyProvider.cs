using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public interface ISeatOccupancyProvider
	{
		IList<OccupancyViewModel> Get (Guid seatId, DateOnly date);
		IList<OccupancyViewModel> GetSeatBookingsForCurrentUser (DateOnlyPeriod dateOnlyPeriod);
	}
}