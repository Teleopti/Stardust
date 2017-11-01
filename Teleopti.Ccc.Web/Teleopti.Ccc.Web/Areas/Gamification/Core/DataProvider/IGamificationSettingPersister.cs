using System;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public interface IGamificationSettingPersister
	{
		GamificationSettingViewModel Persist();
		GamificationDescriptionViewMode PersistDescription(GamificationDescriptionViewMode input);
	}
}