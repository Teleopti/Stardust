using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class RtaInitializor
	{
		private readonly IAdherenceAggregator _adherenceAggregator;
		private readonly IStateStreamSynchronizer _synchronizer;
		private readonly ICurrentDataSource _dataSource;
		private readonly TenantsInitializedInRta _tenants;
		private readonly object _initializeLock = new object();

		public RtaInitializor(
			IAdherenceAggregator adherenceAggregator,
			IStateStreamSynchronizer synchronizer,
			ICurrentDataSource dataSource,
			TenantsInitializedInRta tenants)
		{
			_adherenceAggregator = adherenceAggregator;
			_synchronizer = synchronizer;
			_dataSource = dataSource;
			_tenants = tenants;
		}

		public void EnsureTenantInitialized()
		{
			if (_tenants.IsInitialized(_dataSource.CurrentName()))
				return;
			lock (_initializeLock)
			{
				if (_tenants.IsInitialized(_dataSource.CurrentName()))
					return;

				_adherenceAggregator.Initialize();
				_synchronizer.Initialize();

				_tenants.Initialized(_dataSource.CurrentName());
			}
		}

	}
}