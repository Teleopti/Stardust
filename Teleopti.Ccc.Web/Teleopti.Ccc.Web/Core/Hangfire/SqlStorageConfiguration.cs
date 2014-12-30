using System;
using Hangfire;
using Hangfire.SqlServer;
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

		public void ConfigureStorage(IBootstrapperConfiguration configuration)
		{
			configuration.UseSqlServerStorage(
				_config.ConnectionStrings["Hangfire"].ConnectionString,
				new SqlServerStorageOptions
				{
					PrepareSchemaIfNecessary = false,
					QueuePollInterval = TimeSpan.FromSeconds(1)
				}
				);
		}
	}
}