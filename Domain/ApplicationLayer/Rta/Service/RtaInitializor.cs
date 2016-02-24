using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class RtaInitializor
	{
		private readonly StateStreamSynchronizer _synchronizer;
		private readonly ICurrentDataSource _dataSource;
		private readonly TenantsInitializedInRta _tenants;
		private readonly object _initializeLock = new object();

		public RtaInitializor(
			StateStreamSynchronizer synchronizer,
			ICurrentDataSource dataSource,
			TenantsInitializedInRta tenants)
		{
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

				_synchronizer.Initialize();

				_tenants.Initialized(_dataSource.CurrentName());
			}
		}
	}
}