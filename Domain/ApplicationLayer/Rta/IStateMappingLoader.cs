using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IStateMappingLoader
	{
		IEnumerable<StateMapping> Load();
	}
}