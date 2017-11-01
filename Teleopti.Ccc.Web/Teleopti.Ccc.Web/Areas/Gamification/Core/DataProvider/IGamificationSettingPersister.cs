using System;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.core.DataProvider
{
	public interface IGamificationSettingPersister
	{
		GamificationViewModel Persist();
	}
}