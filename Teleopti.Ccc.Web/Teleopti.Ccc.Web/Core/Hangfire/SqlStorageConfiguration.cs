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

		public void ConfigureStorage()
		{
			var options = new SqlServerStorageOptions
			{
				PrepareSchemaIfNecessary = false,
				QueuePollInterval = TimeSpan.FromSeconds(_config.ReadValue("HangfireQueuePollIntervalSeconds", 2)),
				JobExpirationCheckInterval = TimeSpan.FromSeconds(_config.ReadValue("HangfireJobExpirationCheckIntervalSeconds", 900))
			};
			GlobalConfiguration.Configuration.UseSqlServerStorage(_config.ConnectionString("Hangfire"), options);
		}
	}
}