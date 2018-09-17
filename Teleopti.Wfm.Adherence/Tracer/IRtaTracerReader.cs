using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.Tracer
{
	public interface IRtaTracerReader
	{
		IEnumerable<RtaTracerLog<T>> ReadOfType<T>();
	}
}