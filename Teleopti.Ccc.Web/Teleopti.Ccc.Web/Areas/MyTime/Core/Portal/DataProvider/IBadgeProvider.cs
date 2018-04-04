using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider
{
	public interface IBadgeProvider
	{
		IEnumerable<BadgeViewModel> GetBadges(DateOnlyPeriod period);
	}
}