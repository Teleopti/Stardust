using System;
using System.Threading.Tasks;
using Hangfire.Server;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Ccc.Infrastructure.RealTimeAdherence.Domain
{
	public class RtaEventStoreSynchronizerProcess : IBackgroundProcess
	{
		private readonly IRtaEventStoreSynchronizer _synchronizer;
		private readonly StateQueueTenants _tenants;
		private readonly IDistributedLockAcquirer _distributedLock;

		public RtaEventStoreSynchronizerProcess(
			IRtaEventStoreSynchronizer synchronizer,
			StateQueueTenants tenants, 
			IDistributedLockAcquirer distributedLock)
		{
			_synchronizer = synchronizer;
			_tenants = tenants;
			_distributedLock = distributedLock;
		}

		public void Execute(BackgroundProcessContext context)
		{
			Parallel.ForEach(_tenants.ActiveTenants(), Synchronize);
			context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(1));
		}

		[TenantScope]
		protected virtual void Synchronize(string tenant)
		{
			_distributedLock.TryLockForTypeOf(this, () =>
			{
				_synchronizer.Synchronize();
			});
		}
	}
}