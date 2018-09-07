using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Tracer
{
	public interface IRtaTracerReader
	{
		IEnumerable<RtaTracerLog<T>> ReadOfType<T>();
	}
}