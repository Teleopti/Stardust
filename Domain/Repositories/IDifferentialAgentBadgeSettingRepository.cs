using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IDifferentialAgentBadgeSettingRepository : IRepository<IDifferentialAgentBadgeSettings>
	{
		IEnumerable<IDifferentialAgentBadgeSettings> FindAllBadgeSettingsByDescription();
	}
}