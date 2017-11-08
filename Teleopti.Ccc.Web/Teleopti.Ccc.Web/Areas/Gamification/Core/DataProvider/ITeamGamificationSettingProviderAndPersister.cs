using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public interface ITeamGamificationSettingProviderAndPersister
	{
		IList<TeamGamificationSettingViewModel> GetTeamGamificationSettingViewModels(List<Guid> teamIds);
		TeamGamificationSettingViewModel SetTeamGamificationSetting(TeamGamificationSettingForm input);
	}
}