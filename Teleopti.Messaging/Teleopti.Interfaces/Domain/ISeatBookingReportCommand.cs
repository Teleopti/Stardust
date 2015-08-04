using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeatBookingReportCriteria
	{
		IEnumerable<ISeatMapLocation> Locations { get; set; }
		IEnumerable<ITeam> Teams { get; set; }
		DateOnlyPeriod Period { get; set; }
	}
}