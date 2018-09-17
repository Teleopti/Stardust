using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.Tracer
{
	public interface IRtaTracerConfigPersister
	{
		void UpdateForTenant(string userCode);
		void DeleteForTenant();
		IEnumerable<RtaTracerConfig> ReadAll();
	}
}