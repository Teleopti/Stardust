using System;
using System.Threading.Tasks;
using Autofac;
using Hangfire;
using Hangfire.SqlServer;
using Owin;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	[UseOnToggle(Toggles.RTA_HangfireEventProcessing_31593)]
	[TaskPriority(100)]
	public class HangfireServerStartupTask : IBootstrapperTask
	{
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IConfigReader _config;

		public HangfireServerStartupTask(ILifetimeScope lifetimeScope, IConfigReader config)
		{
			_lifetimeScope = lifetimeScope;
			_config = config;
		}

		public Task Execute(IAppBuilder application)
		{
			application.UseHangfire(c =>
			{
				c.UseSqlServerStorage(
					_config.ConnectionStrings["Hangfire"].ConnectionString,
					new SqlServerStorageOptions {QueuePollInterval = TimeSpan.FromSeconds(1)}
					);
				c.UseAutofacActivator(_lifetimeScope);
				c.UseServer();
			});
			return null;
		}
	}
}