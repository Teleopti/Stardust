using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Insights.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider
{
	public interface IPermissionProvider
	{
		InsightsPermission GetInsightsPermission(IPerson person, DateOnly? date);
	}
}