using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public interface IGamificationSettingProvider
	{
		IGamificationSetting GetGamificationSetting();
		GamificationSettingViewModel GetGamificationSetting(Guid id);
		IList<GamificationDescriptionViewModel> GetGamificationList();
	}
}