using System;
using System.Linq;
using Autofac;
using Hangfire;
using Hangfire.Common;
using Hangfire.SqlServer;
using Hangfire.States;
using Hangfire.Storage;
using log4net;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireStarter
	{
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IConfigReader _config;
		private readonly EventSerializerSettings _serializerSettings;
		private static readonly ILog logger = LogManager.GetLogger(typeof(HangfireStarter));

		public HangfireStarter(ILifetimeScope lifetimeScope, IConfigReader config, EventSerializerSettings serializerSettings)
		{
			_lifetimeScope = lifetimeScope;
			_config = config;
			_serializerSettings = serializerSettings;
		}

		// GOSH.. Sooo much text...
		public void Start(string connectionString)
		{
			JobHelper.SetSerializerSettings(_serializerSettings);

			var jobExpiration = _config.ReadValue("HangfireJobExpirationSeconds", TimeSpan.FromHours(1).TotalSeconds);
			var pollInterval = _config.ReadValue("HangfireQueuePollIntervalSeconds", TimeSpan.FromSeconds(5).TotalSeconds);
			var jobExpirationCheck = _config.ReadValue("HangfireJobExpirationCheckIntervalSeconds", TimeSpan.FromMinutes(15).TotalSeconds);
			var dashboardStatistics = _config.ReadValue("HangfireDashboardStatistics", false);
			var dashboardCounters = _config.ReadValue("HangfireDashboardCounters", false);

			int defaultCountersAggregateInterval;
			if (dashboardStatistics || dashboardCounters)
				defaultCountersAggregateInterval = (int)new SqlServerStorageOptions().CountersAggregateInterval.TotalSeconds;
			else
			{
				// for optimization. disable the counter aggregator, 
				// because we dont have any counters any more
				// well, today anyway...
				// NOT FUTURE PROOF! DANGER DANGER!
				defaultCountersAggregateInterval = (int)TimeSpan.FromDays(1).TotalSeconds;
			}
			var countersAggregateInterval = _config.ReadValue("HangfireCountersAggregateIntervalSeconds", defaultCountersAggregateInterval);

			GlobalConfiguration.Configuration.UseSqlServerStorage(
				connectionString,
				new SqlServerStorageOptions
				{
					PrepareSchemaIfNecessary = false,
					QueuePollInterval = TimeSpan.FromSeconds(pollInterval),
					JobExpirationCheckInterval = TimeSpan.FromSeconds(jobExpirationCheck),
					CountersAggregateInterval = TimeSpan.FromSeconds(countersAggregateInterval)
				});

			GlobalConfiguration.Configuration.UseAutofacActivator(_lifetimeScope);

			// for optimization, only add the filters that we currently use
			// NOT FUTURE PROOF! DANGER DANGER!
			GlobalJobFilters.Filters.Clear();
			GlobalJobFilters.Filters.Add(new JobExpirationTimeAttribute
			{
				JobExpirationTimeoutSeconds = (int) jobExpiration
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

			if (logger.IsDebugEnabled)
			{
				var hangfireConfigLog = "Hangfire setup: ";
				hangfireConfigLog += $"{nameof(jobExpiration)}={jobExpiration}, ";
				hangfireConfigLog += $"{nameof(pollInterval)}={pollInterval}, ";
				hangfireConfigLog += $"{nameof(jobExpirationCheck)}={jobExpirationCheck}, ";
				hangfireConfigLog += $"{nameof(dashboardStatistics)}={dashboardStatistics}, ";
				hangfireConfigLog += $"{nameof(dashboardCounters)}={dashboardCounters}, ";
				hangfireConfigLog += $"{nameof(countersAggregateInterval)}={countersAggregateInterval}, ";
				logger.Debug(hangfireConfigLog);
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