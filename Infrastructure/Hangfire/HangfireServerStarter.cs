using System;
using System.Linq;
using Hangfire;
using Owin;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireServerStarter
	{
		private readonly HangfireStarter _starter;
		private readonly IConfigReader _config;

		public HangfireServerStarter(
			HangfireStarter starter, 
			IConfigReader config)
		{
			_starter = starter;
			_config = config;
		}

		public void Start(IAppBuilder app)
		{
			_starter.Start(_config.ConnectionString("Hangfire"));
			app.UseHangfireServer(new BackgroundJobServerOptions
			{
				WorkerCount = _config.ReadValue("HangFireWorkerCount", 8),
				Queues = Queues.OrderOfPriority().ToArray()
			});
		}
	}
}