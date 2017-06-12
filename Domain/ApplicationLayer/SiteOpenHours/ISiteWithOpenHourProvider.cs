using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours
{
	public interface ISiteWithOpenHourProvider
	{
		IEnumerable<SiteViewModel> GetSitesWithOpenHour();
	}
}
