using Hangfire;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public interface IHangfireClientStarter
	{
		void Start();
	}

	public class NoHangfireClient : IHangfireClientStarter
	{
		public void Start()
		{
		}
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
			GlobalConfiguration.Configuration.UseSqlServerStorage(_config.AppConfig("Hangfire"));
		}
	}
}