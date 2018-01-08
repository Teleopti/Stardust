using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Mapping
{
	public interface IGamificationSettingMapper
	{
		GamificationSettingViewModel Map(IGamificationSetting setting);
	}
}