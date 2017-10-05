using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeTraceReader : IRtaTracerReader
	{
		private readonly List<object> _data = new List<object>();

		public FakeTraceReader Has<T>(params RtaTracerLog<T>[] datas)
		{
			_data.AddRange(datas);
			return this;
		}
		
		public IEnumerable<RtaTracerLog<T>> ReadOfType<T>()
		{
			return _data.OfType<RtaTracerLog<T>>().ToArray();
		}
	}
}