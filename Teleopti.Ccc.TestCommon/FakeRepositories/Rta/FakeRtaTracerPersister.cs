using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeRtaTracerPersister : IRtaTracerReader, IRtaTracerWriter
	{
		private readonly List<object> _data = new List<object>();

		public FakeRtaTracerPersister Has<T>(params RtaTracerLog<T>[] datas)
		{
			_data.AddRange(datas);
			return this;
		}
		
		public IEnumerable<RtaTracerLog<T>> ReadOfType<T>()
		{
			return _data.OfType<RtaTracerLog<T>>().ToArray();
		}

		public void Write<T>(RtaTracerLog<T> log)
		{
			_data.Add(log);
		}
	}
}