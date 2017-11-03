using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public interface IGamificationSettingProvider
	{
		GamificationSettingViewModel GetGamificationSetting(Guid id);
		IList<GamificationDescriptionViewModel> GetGamificationList();
	}
}