using System;
using Autofac;
using Hangfire;
using Hangfire.Server;
using Owin;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	[CLSCompliant(false)]
	public class HangfireServerStarter : IHangfireServerStarter
	{
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IHangfireServerStorageConfiguration _storageConfiguration;
		private readonly IConfigReader _config;

		public HangfireServerStarter(ILifetimeScope lifetimeScope, IHangfireServerStorageConfiguration storageConfiguration, IConfigReader config)
		{
			_lifetimeScope = lifetimeScope;
			_storageConfiguration = storageConfiguration;
			_config = config;
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

			GlobalJobFilters.Filters.Clear();
			GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
			{
				Attempts = _config.ReadValue("HangfireAutomaticRetryAttempts", 3),
				OnAttemptsExceeded = AttemptsExceededAction.Delete
			});
			GlobalJobFilters.Filters.Add(new JobExpirationTimeAttribute
			{
				JobExpirationTimeoutSeconds = _config.ReadValue("HangfireJobExpirationSeconds", 600),
			});

			app.UseHangfireServer(new BackgroundJobServerOptions
			{
				WorkerCount = setThisToOneAndErikWillHuntYouDownAndKillYouSlowlyAndPainfully
			});

			if (_config.ReadValue("HangfireDashboard", false))
			{
				GlobalJobFilters.Filters.Add(new StatisticsHistoryAttribute());
				app.UseHangfireDashboard();
			}

		}
	}
}