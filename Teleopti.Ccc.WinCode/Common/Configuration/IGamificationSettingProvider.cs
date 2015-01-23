using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration
{
	public interface IGamificationSettingProvider
	{
		IEnumerable<IGamificationSetting> GetGamificationSettingsEmptyNotIncluded();
		IEnumerable<IGamificationSetting> GetGamificationSettingsEmptyIncluded();
	}
}