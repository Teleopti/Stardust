using Hangfire;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public interface IHangfireClientStarter
	{
		void Start();
	}
	
	public class HangfireClientStarter : IHangfireClientStarter
	{
		private readonly IConfigReader _config;

		public HangfireClientStarter(IConfigReader config)
		{
			_config = config;
		}

		public void Start()
		{
			var connectionString = _config.ConnectionString("Hangfire");
			if (string.IsNullOrEmpty(connectionString))
				connectionString = _config.AppConfig("Hangfire"); //WHA..t?
			GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString);
		}
	}
}