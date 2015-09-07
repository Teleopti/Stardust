using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class RtaStarter
	{
		private readonly IAdherenceAggregator _adherenceAggregator;
		private readonly IStateStreamSynchronizer _synchronizer;
		private readonly ICurrentDataSource _dataSource;
		private readonly IList<string> _initialized = new List<string>();
		private readonly object _initializeLock = new object();

		public RtaStarter(
			IAdherenceAggregator adherenceAggregator,
			IStateStreamSynchronizer synchronizer,
			ICurrentDataSource dataSource)
		{
			_adherenceAggregator = adherenceAggregator;
			_synchronizer = synchronizer;
			_dataSource = dataSource;
		}

		public void EnsureTenantInitialized()
		{
			if (_initialized.Contains(_dataSource.CurrentName()))
				return;
			lock (_initializeLock)
			{
				if (_initialized.Contains(_dataSource.CurrentName()))
					return;

				_adherenceAggregator.Initialize();
				_synchronizer.Initialize();
				_initialized.Add(_dataSource.CurrentName());
			}
		}
	}
}