using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Server;
using log4net;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.Logon.Aspects;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class StateQueueWorker : IBackgroundProcess
	{
		private readonly Domain.ApplicationLayer.Rta.Service.Rta _rta;
		private readonly StateQueueTenants _tenants;
		private readonly IDistributedLockAcquirer _distributedLock;
		private readonly IRtaTracer _tracer;

		public StateQueueWorker(
			Domain.ApplicationLayer.Rta.Service.Rta rta,
			StateQueueTenants tenants,
			IDistributedLockAcquirer distributedLock,
			IRtaTracer tracer)
		{
			_rta = rta;
			_tenants = tenants;
			_distributedLock = distributedLock;
			_tracer = tracer;
		}

		public void Execute(BackgroundProcessContext context)
		{
			Parallel.ForEach(_tenants.ActiveTenants(), QueueIteration);
			context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(100));
		}

		[TenantScope]
		protected virtual void QueueIteration(string tenant)
		{
			_tracer.ProcessProcessing();
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