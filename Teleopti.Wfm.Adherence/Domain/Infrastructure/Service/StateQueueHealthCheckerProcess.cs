using System;
using System.Threading.Tasks;
using Hangfire.Server;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Domain.Infrastructure.Service
{
	public class StateQueueHealthCheckerProcess : IBackgroundProcess
	{
		private readonly IStateQueueHealthChecker _health;
		private readonly ActiveTenants _tenants;

		public StateQueueHealthCheckerProcess(
			IStateQueueHealthChecker health,
			ActiveTenants tenants)
		{
			_health = health;
			_tenants = tenants;
		}

		public void Execute(BackgroundProcessContext context)
		{
			try
			{
				Parallel.ForEach(_tenants.Tenants(), Check);
			}
			finally
			{
				context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(1));
			}
		}

		[TenantScope]
		protected virtual void Check(string tenant)
		{
			_health.Check();
		}
	}
}