using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.RealTimeAdherence.Tracer;

namespace Teleopti.Ccc.TestCommon.FakeRepositories.Rta
{
	public class FakeRtaTracerConfigPersister : IRtaTracerConfigPersister
	{
		private readonly ICurrentDataSource _dataSource;
		private IEnumerable<RtaTracerConfig> _data = new RtaTracerConfig[] { };

		public FakeRtaTracerConfigPersister(ICurrentDataSource dataSource)
		{
			_dataSource = dataSource;
		}

		public IEnumerable<RtaTracerConfig> ReadAll()
		{
			return _data.ToArray();
		}

		public void UpdateForTenant(string userCode)
		{
			if (_dataSource.CurrentName() == null)
				throw new Exception("Tenant scope is required (not null constraint exception in a real db)");
			_data = _data
				.Except(x => x.Tenant == _dataSource.CurrentName())
				.Append(new RtaTracerConfig
				{
					Tenant = _dataSource.CurrentName(),
					UserCode = userCode
				})
				.ToArray();
		}

		public void DeleteForTenant()
		{
			if (_dataSource.CurrentName() == null)
				throw new Exception("Tenant scope is required (not null constraint exception in a real db)");
			_data = _data
				.Except(x => x.Tenant == _dataSource.CurrentName())
				.ToArray();
		}
	}
}