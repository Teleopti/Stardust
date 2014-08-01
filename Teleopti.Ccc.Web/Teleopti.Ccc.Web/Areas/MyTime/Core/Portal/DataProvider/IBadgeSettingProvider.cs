using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider
{
	public interface IBadgeSettingProvider
	{
		IAgentBadgeThresholdSettings GetBadgeSettings();
	}
}