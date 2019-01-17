using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire.Server;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Wfm.Adherence.Historical.Infrastructure
{
	public class RtaEventStoreUpgraderProcess : IBackgroundProcess
	{
		private readonly RtaEventStoreUpgrader _upgrader;
		private readonly ActiveTenants _tenants;
		private readonly IDistributedLockAcquirer _distributedLock;
		private readonly HashSet<string> _upgradedTenants = new HashSet<string>();

		public RtaEventStoreUpgraderProcess(
			RtaEventStoreUpgrader upgrader,
			ActiveTenants tenants,
			IDistributedLockAcquirer distributedLock)
		{
			_upgrader = upgrader;
			_tenants = tenants;
			_distributedLock = distributedLock;
		}

		public void Execute(BackgroundProcessContext context)
		{
			try
			{
				Parallel.ForEach(_tenants.Tenants(), Upgrade);
			}
			finally
			{
				context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromMinutes(1));
			}
		}

		[TenantScope]
		protected virtual void Upgrade(string tenant)
		{
			_distributedLock.TryLockForTypeOf(this, () =>
			{
				if (_upgradedTenants.Contains(tenant))
					return;
				_upgrader.Upgrade();
				_upgradedTenants.Add(tenant);
			});
		}
	}
}