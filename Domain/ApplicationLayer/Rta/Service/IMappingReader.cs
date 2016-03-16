using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IMappingReader
	{
		IEnumerable<Mapping> Read();
	}
}