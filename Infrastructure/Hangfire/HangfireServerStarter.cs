using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Hangfire.Server;
using Owin;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireServerStarter
	{
		private readonly HangfireStarter _starter;
		private readonly IConfigReader _config;
		private readonly IEnumerable<IBackgroundProcess> _backgroundProcesses;

		public HangfireServerStarter(
			HangfireStarter starter,
			IConfigReader config,
			IEnumerable<IBackgroundProcess> backgroundProcesses)
		{
			_starter = starter;
			_config = config;
			_backgroundProcesses = backgroundProcesses;
		}

		public void Start(IAppBuilder app)
		{
			_starter.Start(_config.ConnectionString("Hangfire"));

			var workerCount = _config.ReadValue("HangFireWorkerCount", 8);
			var options = workerCount > 0
				? new BackgroundJobServerOptions
				{
					WorkerCount = workerCount,
					Queues = Queues.OrderOfPriority().ToArray()
				}
				: new BackgroundJobServerOptions
				{
					WorkerCount = 1,
					Queues = new[] {"idle_worker_queue"}
				};

			app.UseHangfireServer(
				options,
				_backgroundProcesses.ToArray()
			);
		}
	}
}