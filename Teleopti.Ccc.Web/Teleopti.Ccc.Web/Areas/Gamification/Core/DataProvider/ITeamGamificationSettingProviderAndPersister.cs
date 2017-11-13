using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public interface ITeamGamificationSettingProviderAndPersister
	{
		IList<TeamGamificationSettingViewModel> GetTeamGamificationSettingViewModels(List<Guid> siteIds);
		List<TeamGamificationSettingViewModel> SetTeamsGamificationSetting(TeamsGamificationSettingForm input);
		TeamGamificationSettingViewModel SetTeamGamificationSetting(Guid teamId, Guid inputGamificationSettingId);
	}
}