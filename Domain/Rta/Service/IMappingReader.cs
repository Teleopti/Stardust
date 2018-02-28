using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Rta.Service
{
	public interface IMappingReader
	{
		IEnumerable<Mapping> Read();
	}
}