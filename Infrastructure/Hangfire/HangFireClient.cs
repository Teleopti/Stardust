using Hangfire;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public interface IHangFireClient
	{
		void Configure();
	}

	public class NoHangFireClient : IHangFireClient
	{
		public void Configure()
		{
		}
	}

	public class HangFireClient : IHangFireClient
	{
		private readonly IConfigReader _config;

		public HangFireClient(IConfigReader config)
		{
			_config = config;
		}

		public void Configure()
		{
			GlobalConfiguration.Configuration.UseSqlServerStorage(_config.AppConfig("Hangfire"));
		}
	}
}