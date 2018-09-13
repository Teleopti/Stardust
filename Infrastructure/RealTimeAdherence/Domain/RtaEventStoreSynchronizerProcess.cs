using System;
using System.Threading.Tasks;
using Hangfire.Server;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;

namespace Teleopti.Ccc.Infrastructure.RealTimeAdherence.Domain
{
	public class RtaEventStoreSynchronizerProcess : IBackgroundProcess
	{
		private readonly IRtaEventStoreSynchronizer _synchronizer;
		private readonly StateQueueTenants _tenants;
		private readonly IDistributedLockAcquirer _distributedLock;

		public RtaEventStoreSynchronizerProcess(IRtaEventStoreSynchronizer synchronizer,
			StateQueueTenants tenants, IDistributedLockAcquirer distributedLock)
		{
			_synchronizer = synchronizer;
			_tenants = tenants;
			_distributedLock = distributedLock;
		}

		public void Execute(BackgroundProcessContext context)
		{	
			Parallel.ForEach(_tenants.ActiveTenants(), Synchronize);			
			context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromMinutes(1));
		}

		[TenantScope]
		[FullPermissions]
		protected virtual void Synchronize(string tenant)
		{
			_distributedLock.TryLockForTypeOf(this, () =>
			{
				_synchronizer.Synchronize();
			});
		}		
	}
}
