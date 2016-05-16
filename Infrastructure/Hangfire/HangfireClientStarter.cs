using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireClientStarter
	{
		private readonly HangfireStarter _starter;
		private readonly IConfigReader _config;

		public HangfireClientStarter(HangfireStarter starter, IConfigReader config)
		{
			_starter = starter;
			_config = config;
		}

		public void Start()
		{
			var connectionString = _config.ConnectionString("Hangfire");
			if (string.IsNullOrEmpty(connectionString))
				connectionString = _config.AppConfig("Hangfire"); //WHA..t?

			_starter.Start(connectionString);
		}
	}
}