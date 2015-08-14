using System;
using Hangfire;
using Hangfire.SqlServer;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Web.Core.Hangfire
{
	[CLSCompliant(false)]
	public class SqlStorageConfiguration : IHangfireServerStorageConfiguration
	{
		private readonly IConfigReader _config;

		public SqlStorageConfiguration(IConfigReader config)
		{
			_config = config;
		}

		public void ConfigureStorage(bool useDashboard)
		{
			// for optimization. basically disable the counter aggregator, 
			// because we dont have any counters any more in the current version
			// NOT FUTURE PROOF! DANGER DANGER!
			var defaultCountersAggregateInterval = 60 * 60 * 24;
			if (useDashboard)
				defaultCountersAggregateInterval = (int)new SqlServerStorageOptions().CountersAggregateInterval.TotalSeconds;

			var options = new SqlServerStorageOptions
			{
				PrepareSchemaIfNecessary = false,
				QueuePollInterval = TimeSpan.FromSeconds(_config.ReadValue("HangfireQueuePollIntervalSeconds", 2)),
				JobExpirationCheckInterval = TimeSpan.FromSeconds(_config.ReadValue("HangfireJobExpirationCheckIntervalSeconds", 60 * 15)),
				CountersAggregateInterval = TimeSpan.FromSeconds(_config.ReadValue("HangfireCountersAggregateIntervalSeconds", defaultCountersAggregateInterval))
			};

			GlobalConfiguration.Configuration.UseSqlServerStorage(_config.ConnectionString("Hangfire"), options);
		}
	}
}