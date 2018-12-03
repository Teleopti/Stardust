using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface ISeatBookingReportCriteria
	{
		IEnumerable<ISeatMapLocation> Locations { get; set; }
		IEnumerable<ITeam> Teams { get; set; }
		DateOnlyPeriod Period { get; set; }
		bool ShowOnlyUnseated { get; set; }
	}
}