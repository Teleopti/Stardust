using System;
using System.Linq;
using Autofac;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.States;
using Hangfire.Storage;
using Owin;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireServerStarter
	{
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IConfigReader _config;
		private readonly ActivityChangesChecker _activityChangesChecker;

		public HangfireServerStarter(ILifetimeScope lifetimeScope, IConfigReader config, ActivityChangesChecker activityChangesChecker)
		{
			_lifetimeScope = lifetimeScope;
			_config = config;
			_activityChangesChecker = activityChangesChecker;
		}

		const int setThisToOneAndErikWillHuntYouDownAndKillYouSlowlyAndPainfully = 4;
		// But really.. using one worker will:
		// - reduce performance of RTA too much
		// - will be a one-way street where the code will never handle the concurrency
		// - still wont handle the fact that some customers have more than one server running, 
		//   so you will still have more than one worker

		
		
		// GOSH.. Sooo much text...
		public void Start(IAppBuilder app)
		{
			var retries = _config.ReadValue("HangfireAutomaticRetryAttempts", 3);
			var jobExpiration = _config.ReadValue("HangfireJobExpirationSeconds", 60 * 60 * 24 * 2);
			var pollInterval = _config.ReadValue("HangfireQueuePollIntervalSeconds", 2);
			var jobExpirationCheck = _config.ReadValue("HangfireJobExpirationCheckIntervalSeconds", 60 * 15);
			var dashboard = _config.ReadValue("HangfireDashboard", true);
			var dashboardStatistics = _config.ReadValue("HangfireDashboardStatistics", false);
			var dashboardCounters = _config.ReadValue("HangfireDashboardCounters", false);

			var defaultCountersAggregateInterval = 0;
			if (dashboardStatistics || dashboardCounters)
				defaultCountersAggregateInterval = (int)new SqlServerStorageOptions().CountersAggregateInterval.TotalSeconds;
			else
			{
				// for optimization. disable the counter aggregator, 
				// because we dont have any counters any more
				// well, today anyway...
				// NOT FUTURE PROOF! DANGER DANGER!
				defaultCountersAggregateInterval = 60 * 60 * 24;
			}
			var countersAggregateInterval = _config.ReadValue("HangfireCountersAggregateIntervalSeconds", defaultCountersAggregateInterval);


			GlobalConfiguration.Configuration.UseStorage(
				new SqlStorageWithActivityChangesCheckerComponent(
					_activityChangesChecker,
					_config.ConnectionString("Hangfire"),
					new SqlServerStorageOptions
					{
						PrepareSchemaIfNecessary = false,
						QueuePollInterval = TimeSpan.FromSeconds(pollInterval),
						JobExpirationCheckInterval = TimeSpan.FromSeconds(jobExpirationCheck),
						CountersAggregateInterval = TimeSpan.FromSeconds(countersAggregateInterval)
					}));
			
			GlobalConfiguration.Configuration.UseAutofacActivator(_lifetimeScope);



			// for optimization, only add the filters that we currently use
			// NOT FUTURE PROOF! DANGER DANGER!
			GlobalJobFilters.Filters.Clear();
			GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
			{
				Attempts = retries,
				OnAttemptsExceeded = AttemptsExceededAction.Delete
			});
			GlobalJobFilters.Filters.Add(new JobExpirationTimeAttribute
			{
				JobExpirationTimeoutSeconds = jobExpiration
			});
			// for optimization, only add the history counters as extra if explicitly configured so
			// NOT FUTURE PROOF! DANGER DANGER!
			if (dashboardStatistics)
				GlobalJobFilters.Filters.Add(new StatisticsHistoryAttribute());

			if (!dashboardCounters)
			{
				// for optimization, remove some internal handlers because at this time
				// the only thing they do is insert into the Hangfire.Counters table
				// add handlers that does nothing with the same state name
				// NOT FUTURE PROOF! DANGER DANGER!
				GlobalStateHandlers.Handlers.Remove(GlobalStateHandlers.Handlers.Single(x => x.StateName == SucceededState.StateName));
				GlobalStateHandlers.Handlers.Remove(GlobalStateHandlers.Handlers.Single(x => x.StateName == DeletedState.StateName));
				GlobalStateHandlers.Handlers.Add(new emptyHandler(SucceededState.StateName));
				GlobalStateHandlers.Handlers.Add(new emptyHandler(DeletedState.StateName));
			}



			app.UseHangfireServer(new BackgroundJobServerOptions
			{
				WorkerCount = setThisToOneAndErikWillHuntYouDownAndKillYouSlowlyAndPainfully
			});



			if (dashboard)
				app.UseHangfireDashboard();


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