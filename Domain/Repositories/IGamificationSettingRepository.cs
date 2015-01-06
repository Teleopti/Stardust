using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IGamificationSettingRepository : IRepository<IGamificationSetting>
	{
		IEnumerable<IGamificationSetting> FindAllBadgeSettingsByDescription();
	}
}