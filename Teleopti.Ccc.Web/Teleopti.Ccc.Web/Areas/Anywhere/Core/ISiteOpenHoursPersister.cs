using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface ISiteOpenHoursPersister
	{
		int Persist(IEnumerable<SiteViewModel> sites);
	}
}