using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Islands
{
	public interface IIsland
	{
		IEnumerable<IPerson> AgentsInIsland();
	}
}