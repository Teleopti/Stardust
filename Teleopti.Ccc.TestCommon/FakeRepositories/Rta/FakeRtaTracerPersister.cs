using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence.Tracer;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeRtaTracerPersister : IRtaTracerReader, IRtaTracerWriter
	{
		private readonly ICurrentDataSource _dataSource;
		private readonly List<object> _data = new List<object>();

		public FakeRtaTracerPersister(ICurrentDataSource dataSource)
		{
			_dataSource = dataSource;
		}

		public FakeRtaTracerPersister Has<T>(params RtaTracerLog<T>[] datas)
		{
			datas.ForEach(x => x.Tenant = _dataSource.CurrentName());
			_data.AddRange(datas);
			return this;
		}

		public IEnumerable<RtaTracerLog<T>> ReadOfType<T>()
		{
			return _data.OfType<RtaTracerLog<T>>()
				.Where(x => x.Tenant == _dataSource.CurrentName())
				.ToArray();
		}

		public void Write<T>(RtaTracerLog<T> log)
		{
			_data.Add(log);
		}

		public void Flush()
		{
		}

		public void Clear()
		{
			_data.Clear();
		}

		public void Purge()
		{
		}
	}
}