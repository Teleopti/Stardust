using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Rta.Tracer
{
	public interface IRtaTracerReader
	{
		IEnumerable<RtaTracerLog<T>> ReadOfType<T>();
	}
}