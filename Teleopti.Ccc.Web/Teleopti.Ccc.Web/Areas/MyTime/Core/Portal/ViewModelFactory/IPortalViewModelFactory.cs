using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.ViewModelFactory
{
	public interface IPortalViewModelFactory
	{
		PortalViewModel CreatePortalViewModel();
		IEnumerable<BadgeViewModel> GetBadges(DateOnlyPeriod period);
	}
}