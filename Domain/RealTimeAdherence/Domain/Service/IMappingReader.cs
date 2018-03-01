using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public interface IMappingReader
	{
		IEnumerable<Mapping> Read();
	}
}