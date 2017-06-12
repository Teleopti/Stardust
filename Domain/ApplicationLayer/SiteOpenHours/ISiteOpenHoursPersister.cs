using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours
{
	public interface ISiteOpenHoursPersister
	{
		int Persist(IEnumerable<SiteViewModel> sites);
	}
}
