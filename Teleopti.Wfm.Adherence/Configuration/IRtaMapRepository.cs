using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.Configuration
{
	public interface IRtaMapRepository : IRepository<IRtaMap>
	{
		IEnumerable<IRtaMap> LoadAllCompleteGraph();
	}
}