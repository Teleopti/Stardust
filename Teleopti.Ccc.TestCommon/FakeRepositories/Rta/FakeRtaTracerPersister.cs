using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Support.Library;
using Teleopti.Wfm.Adherence.Tracer;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeRtaTracerPersister : IRtaTracerReader, IRtaTracerWriter
	{
		private readonly ICurrentDataSource _dataSource;
		private readonly List<object> _data = new List<object>();
		private string _tenant;

		public FakeRtaTracerPersister(ICurrentDataSource dataSource)
		{
			_dataSource = dataSource;
		}

		public FakeRtaTracerPersister WithCurrentTenant()
		{
			_tenant = _dataSource.CurrentName();
			return this;
		}
		
		public FakeRtaTracerPersister Has<T>(params RtaTracerLog<T>[] datas)
		{
			if (_tenant != null)
				datas.ForEach(x => x.Tenant = _tenant);
			_data.AddRange(datas);
			return this;
		}

		public IEnumerable<RtaTracerLog<T>> ReadOfType<T>()
		{
			return _data.OfType<RtaTracerLog<T>>()
				.Where(x => x.Tenant == _dataSource.CurrentName() || x.Tenant == null)
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