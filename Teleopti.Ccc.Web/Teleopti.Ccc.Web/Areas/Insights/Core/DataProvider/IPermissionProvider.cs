using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Insights.Models;

namespace Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider
{
	public interface IPermissionProvider
	{
		InsightsPermission GetInsightsPermission(IPerson person, DateOnly? date);
	}
}