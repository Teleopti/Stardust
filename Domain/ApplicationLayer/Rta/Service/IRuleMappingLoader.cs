using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IRuleMappingLoader
	{
		IEnumerable<RuleMapping> Cached();
		IEnumerable<RuleMapping> Load();
	}
}