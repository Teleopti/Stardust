using System;
using System.Threading.Tasks;
using Hangfire.Server;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Logon.Aspects;

namespace Teleopti.Wfm.Adherence.States.Infrastructure
{
	public class StateQueueWorker : IBackgroundProcess
	{
		private readonly Rta _rta;
		private readonly StateQueueTenants _tenants;
		private readonly IDistributedLockAcquirer _distributedLock;

		public StateQueueWorker(
			Rta rta,
			StateQueueTenants tenants,
			IDistributedLockAcquirer distributedLock)
		{
			_rta = rta;
			_tenants = tenants;
			_distributedLock = distributedLock;
		}

		public void Execute(BackgroundProcessContext context)
		{
			Parallel.ForEach(_tenants.Tenants(), QueueIteration);
			context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(100));
		}

		[TenantScope]
		protected virtual void QueueIteration(string tenant)
		{
			_distributedLock.TryLockForTypeOf(this, () =>
			{
				bool iterated;
				do
				{
					iterated = _rta.QueueIteration(tenant, new CatchAndLogAll());
				} while (iterated);
			});
		}
	}
}