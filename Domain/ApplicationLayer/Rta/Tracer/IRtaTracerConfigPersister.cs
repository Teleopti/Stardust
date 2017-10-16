using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer
{
	public interface IRtaTracerConfigPersister
	{
		void UpdateForTenant(string userCode);
		void DeleteForTenant();
		IEnumerable<RtaTracerConfig> ReadAll();
	}
}