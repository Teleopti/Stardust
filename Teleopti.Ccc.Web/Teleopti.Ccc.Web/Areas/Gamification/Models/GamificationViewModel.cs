using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Gamification.Models
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenOptionsPage)]

	public class GamificationViewModel: GamificationSettingView
	{
		public GamificationViewModel(IGamificationSetting setting) : base(setting)
		{
		}
	}
}