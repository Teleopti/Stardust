using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ITeamGamificationSettingRepository:IRepository<ITeamGamificationSetting>
	{
		IEnumerable<ITeamGamificationSetting> FindAllTeamGamificationSettingsSortedByTeam();
		ITeamGamificationSetting FindTeamGamificationSettingsByTeam(ITeam myTeam);
	}
}
