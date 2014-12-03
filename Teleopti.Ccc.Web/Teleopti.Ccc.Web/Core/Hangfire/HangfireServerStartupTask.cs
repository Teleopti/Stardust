using System;
using System.Threading.Tasks;
using Autofac;
using Owin;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	[UseOnToggle(Toggles.RTA_HangfireEventProcessing_31593)]
	public class HangfireServerStartupTask : IBootstrapperTask
	{
		private readonly Func<IApplicationData> _applicationData;
		private readonly ILifetimeScope _lifetimeScope;

		public HangfireServerStartupTask(Func<IApplicationData> applicationData, ILifetimeScope lifetimeScope)
		{
			_applicationData = applicationData;
			_lifetimeScope = lifetimeScope;
		}

		public Task Execute(IAppBuilder application)
		{
			//application.UseHangfire(c =>
			//{
			//	var connectionString = _applicationData.Invoke().RegisteredDataSourceCollection.Single().Statistic.ConnectionString;
			//	c.UseSqlServerStorage(connectionString, new SqlServerStorageOptions { QueuePollInterval = TimeSpan.FromSeconds(1) });
			//	c.UseAutofacActivator(_lifetimeScope);
			//	c.UseServer();
			//});
			return null;
		}
	}
}