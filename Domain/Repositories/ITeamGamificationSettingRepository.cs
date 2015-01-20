using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface ITeamGamificationSettingRepository:IRepository<ITeamGamificationSetting>
	{
		IEnumerable<ITeamGamificationSetting> FindAllTeamGamificationSettingsSortedByTeam();
	}
}
