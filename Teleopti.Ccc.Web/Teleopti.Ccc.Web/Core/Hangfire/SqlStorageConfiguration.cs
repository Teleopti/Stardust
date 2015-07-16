using System;
using Hangfire;
using Hangfire.SqlServer;
using Teleopti.Ccc.Domain.MultipleConfig;
using Teleopti.Interfaces.Infrastructure;

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
				QueuePollInterval = TimeSpan.FromSeconds(1),
				JobExpirationCheckInterval =
					TimeSpan.FromSeconds(int.Parse(_config.AppSettings["HangfireJobExpirationCheckIntervalSeconds"]))
			};
			GlobalConfiguration.Configuration.UseSqlServerStorage(_config.ConnectionStrings["Hangfire"].ConnectionString, options);
		}
	}
}