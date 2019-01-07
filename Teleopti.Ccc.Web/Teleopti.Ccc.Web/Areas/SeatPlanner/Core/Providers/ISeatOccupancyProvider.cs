using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;


namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public interface ISeatOccupancyProvider
	{
		IList<GroupedOccupancyViewModel> Get (IList<Guid> seatIds, DateOnly date);
		IList<OccupancyViewModel> GetSeatBookingsForScheduleDays (DateOnlyPeriod dateOnlyPeriod, IPerson person);
	}
}