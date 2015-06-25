using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Outbound.Models;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider
{
	public interface IOutboundActivityProvider
	{
		IEnumerable<ActivityViewModel> GetAll();
	}
}