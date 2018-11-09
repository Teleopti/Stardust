using System;
using System.Threading.Tasks;
using Hangfire.Server;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Wfm.Adherence.Domain.Events;

namespace Teleopti.Wfm.Adherence.Domain.Infrastructure
{
	public class RtaEventStoreSynchronizerProcess : IBackgroundProcess
	{
		private readonly IRtaEventStoreSynchronizer _synchronizer;
		private readonly ActiveTenants _tenants;

		public RtaEventStoreSynchronizerProcess(
			IRtaEventStoreSynchronizer synchronizer,
			ActiveTenants tenants)
		{
			_synchronizer = synchronizer;
			_tenants = tenants;
		}

		public void Execute(BackgroundProcessContext context)
		{
			try
			{
				Parallel.ForEach(_tenants.Tenants(), Synchronize);
			}
			finally
			{
				context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(10));
			}
		}

		[TenantScope]
		protected virtual void Synchronize(string tenant) => _synchronizer.Synchronize();
	}
}