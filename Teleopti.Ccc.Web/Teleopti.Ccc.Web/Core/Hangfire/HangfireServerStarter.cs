using System;
using Autofac;
using Hangfire;
using Owin;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	[CLSCompliant(false)]
	public class HangfireServerStarter : IHangfireServerStarter
	{
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IHangfireServerStorageConfiguration _storageConfiguration;

		public HangfireServerStarter(ILifetimeScope lifetimeScope, IHangfireServerStorageConfiguration storageConfiguration)
		{
			_lifetimeScope = lifetimeScope;
			_storageConfiguration = storageConfiguration;
		}

		const int setThisToOneAndErikWillHuntYouDownAndKillYouSlowlyAndPainfully = 4;
		// But really.. using one worker will:
		// - reduce performance of RTA too much
		// - will be a one-way street where the code will never handle the concurrency
		// - still wont handle the fact that some customers have more than one server running, 
		//   so you will still have more than one worker

		public void Start(IAppBuilder app)
		{
			_storageConfiguration.ConfigureStorage();
			GlobalConfiguration.Configuration.UseAutofacActivator(_lifetimeScope);
			GlobalJobFilters.Filters.Add(new JobExpirationTimeAttribute());
			app.UseHangfireServer(new BackgroundJobServerOptions
			{
				WorkerCount = setThisToOneAndErikWillHuntYouDownAndKillYouSlowlyAndPainfully
			});
			app.UseHangfireDashboard();
		}
	}
}