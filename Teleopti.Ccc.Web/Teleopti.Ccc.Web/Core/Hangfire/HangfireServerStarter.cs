using System;
using Autofac;
using Hangfire;
using Hangfire.SqlServer;
using Owin;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	public class HangfireServerStarter : IHangfireServerStarter
	{
		private readonly ILifetimeScope _lifetimeScope;

		public HangfireServerStarter(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope;
		}

		public void Start(IAppBuilder application, string connectionString)
		{
			application.UseHangfire(c =>
			{
				c.UseSqlServerStorage(connectionString, new SqlServerStorageOptions {QueuePollInterval = TimeSpan.FromSeconds(1)});
				c.UseAutofacActivator(_lifetimeScope);
				c.UseServer();
			});
		}
	}
}