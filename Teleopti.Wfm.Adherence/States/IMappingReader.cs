using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.States
{
	public interface IMappingReader
	{
		IEnumerable<Mapping> Read();
	}
}