using System;
using System.Linq;
using Autofac;
using Hangfire;
using Hangfire.States;
using Hangfire.Storage;
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
			var useDashboard = _config.ReadValue("HangfireDashboard", false);

			_storageConfiguration.ConfigureStorage(useDashboard);

			GlobalConfiguration.Configuration.UseAutofacActivator(_lifetimeScope);

			GlobalJobFilters.Filters.Clear();
			GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
			{
				Attempts = _config.ReadValue("HangfireAutomaticRetryAttempts", 3),
				OnAttemptsExceeded = AttemptsExceededAction.Delete
			});
			GlobalJobFilters.Filters.Add(new JobExpirationTimeAttribute
			{
				JobExpirationTimeoutSeconds = _config.ReadValue("HangfireJobExpirationSeconds", 60 * 10),
			});

			app.UseHangfireServer(new BackgroundJobServerOptions
			{
				WorkerCount = setThisToOneAndErikWillHuntYouDownAndKillYouSlowlyAndPainfully
			});

			if (useDashboard)
			{
				// for optimization, only add the history counters as extra if explicitly configured so
				if (_config.ReadValue("HangfireDashboardStatistics", false))
					GlobalJobFilters.Filters.Add(new StatisticsHistoryAttribute());
				app.UseHangfireDashboard();
			}
			else
			{
				// for optimization, remove some internal handlers because at this time
				// the only thing they do is insert into the Hangfire.Counters table
				// add handlers that does nothing with the same state name
				// if you have read this far, you might want to check the SqlStorageConfiguration
				// class aswell
				// NOT FUTURE PROOF! DANGER DANGER!
				GlobalStateHandlers.Handlers.Remove(GlobalStateHandlers.Handlers.Single(x => x.StateName == SucceededState.StateName));
				GlobalStateHandlers.Handlers.Remove(GlobalStateHandlers.Handlers.Single(x => x.StateName == DeletedState.StateName));
				GlobalStateHandlers.Handlers.Add(new emptyHandler(SucceededState.StateName));
				GlobalStateHandlers.Handlers.Add(new emptyHandler(DeletedState.StateName));
			}

		}
		
		private class emptyHandler : IStateHandler
		{
			public emptyHandler(string stateName)
			{
				StateName = stateName;
			}

			public void Apply(ApplyStateContext context, IWriteOnlyTransaction transaction)
			{
			}

			public void Unapply(ApplyStateContext context, IWriteOnlyTransaction transaction)
			{
			}

			public string StateName { get; set; }
		}

	}
}