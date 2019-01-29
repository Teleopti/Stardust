using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ITeamGamificationSettingRepository:IRepository<ITeamGamificationSetting>
	{
		IEnumerable<ITeamGamificationSetting> FindAllTeamGamificationSettingsSortedByTeam();
		ITeamGamificationSetting FindTeamGamificationSettingsByTeam(ITeam myTeam);
		IEnumerable<ITeamGamificationSetting> FetchTeamGamificationSettings(Guid gamificationId);
	}
}
