using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.SeatManagement
{
	public class SeatBookingReportCriteria : ISeatBookingReportCriteria
	{
		public IEnumerable<ISeatMapLocation> Locations { get; set; }
		public IEnumerable<ITeam> Teams { get; set; }
		public DateOnlyPeriod Period { get; set; }

		public SeatBookingReportCriteria()
		{
		}
		public SeatBookingReportCriteria(IEnumerable<ISeatMapLocation> locations, IEnumerable<ITeam> teams,
			DateOnlyPeriod period)
		{
			Locations = locations;
			Teams = teams;
			Period = period;
		}

	}
}