using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Tracer
{
	public interface IRtaTracerConfigPersister
	{
		void UpdateForTenant(string userCode);
		void DeleteForTenant();
		IEnumerable<RtaTracerConfig> ReadAll();
	}
}