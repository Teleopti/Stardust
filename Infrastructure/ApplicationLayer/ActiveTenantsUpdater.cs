using System;
using Hangfire.Server;
using Teleopti.Wfm.Adherence;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class ActiveTenantsUpdater : IBackgroundProcess
	{
		private readonly ActiveTenants _tenants;

		public ActiveTenantsUpdater(ActiveTenants tenants)
		{
			_tenants = tenants;
		}
		
		public void Execute(BackgroundProcessContext context)
		{
			try
			{
				_tenants.Update();
				context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromMinutes(5));
			}
			finally
			{
				context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromMinutes(1));
			}
		}
	}
}