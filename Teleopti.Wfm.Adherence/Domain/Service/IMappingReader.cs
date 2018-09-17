using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.Domain.Service
{
	public interface IMappingReader
	{
		IEnumerable<Mapping> Read();
	}
}