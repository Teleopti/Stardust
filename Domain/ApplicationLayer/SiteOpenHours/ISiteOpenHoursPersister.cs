using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours
{
	public interface ISiteOpenHoursPersister
	{
		int Persist(IEnumerable<SiteViewModel> sites);
	}
}
