using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Server;
using log4net;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
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
		private static readonly ILog Log = LogManager.GetLogger(typeof(StateQueueWorker));

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
					iterated = handleRtaExceptions(() => _rta.QueueIteration(tenant));
				} while (iterated);
			});
		}

		private bool handleRtaExceptions(Func<bool> call)
		{
			try
			{
				return call.Invoke();
			}
			catch (InvalidSourceException e)
			{
				Log.Error("Source id was invalid.", e);
			}
			catch (InvalidPlatformException e)
			{
				Log.Error("Platform id was invalid.", e);
			}
			catch (InvalidUserCodeException e)
			{
				Log.Info("User code was invalid.", e);
			}
			catch (AggregateException e)
			{
				var onlyInvalidUserCode =
					e.AllExceptions()
						.Where(x => x.GetType() != typeof(AggregateException))
						.All(x => x.GetType() == typeof(InvalidUserCodeException));
				if (onlyInvalidUserCode)
					Log.Info("Batch contained invalid user code.", e);
				else
					Log.Error("Unhandled exception occurred processing states from queue", e);
			}
			catch (Exception e)
			{
				Log.Error("Unhandled exception occurred processing states from queue", e);
			}
			return true;
		}
	}
}