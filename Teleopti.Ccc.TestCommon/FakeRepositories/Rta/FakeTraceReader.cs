using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeTraceReader : IRtaTracerReader
	{
		private readonly IList _data = new List<object>();

		public FakeTraceReader Has<T>(RtaTracerLog<T> stuff)
		{
			_data.Add(stuff);
			return this;
		}
		
		public IEnumerable<RtaTracerLog<T>> ReadOfType<T>()
		{
			return _data.OfType<RtaTracerLog<T>>().ToArray();
		}
	}
}