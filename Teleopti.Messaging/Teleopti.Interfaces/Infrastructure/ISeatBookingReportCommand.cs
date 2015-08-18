using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface ISeatBookingReportCriteria
	{
		IEnumerable<ISeatMapLocation> Locations { get; set; }
		IEnumerable<ITeam> Teams { get; set; }
		DateOnlyPeriod Period { get; set; }
	}
}