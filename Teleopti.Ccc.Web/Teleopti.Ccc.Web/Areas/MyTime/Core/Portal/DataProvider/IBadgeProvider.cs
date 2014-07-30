using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider
{
	public interface IBadgeProvider
	{
		IEnumerable<IAgentBadge> GetBadges();
	}
}