using System;
using System.Threading.Tasks;
using Hangfire.Server;
using log4net;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Wfm.Adherence;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Ccc.Infrastructure.RealTimeAdherence.Domain
{
	public class RtaEventStoreSynchronizerProcess : IBackgroundProcess
	{
		private readonly IRtaEventStoreSynchronizer _synchronizer;
		private readonly ActiveTenants _tenants;
		private readonly IDistributedLockAcquirer _distributedLock;

		public RtaEventStoreSynchronizerProcess(
			IRtaEventStoreSynchronizer synchronizer,
			ActiveTenants tenants,
			IDistributedLockAcquirer distributedLock)
		{
			_synchronizer = synchronizer;
			_tenants = tenants;
			_distributedLock = distributedLock;
		}

		public void Execute(BackgroundProcessContext context)
		{
			try
			{
				Parallel.ForEach(_tenants.Tenants(), Synchronize);
			}
			finally
			{
				context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(1));
			}
		}

		[TenantScope]
		protected virtual void Synchronize(string tenant)
		{
			_distributedLock.TryLockForTypeOf(this, () => { _synchronizer.Synchronize(); });
		}
	}
}