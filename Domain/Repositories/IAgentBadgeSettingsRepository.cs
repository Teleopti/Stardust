using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAgentBadgeSettingsRepository : ISettingDataRepository
	{
		IAgentBadgeThresholdSettings GetSettings();
	}
}
