using System.Collections.Generic;
using System.Linq;
using Hangfire;
using log4net;
using Owin;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Infrastructure.Rta;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireServerStarter
	{
		private readonly HangfireStarter _starter;
		private readonly IConfigReader _config;
		private readonly StateQueueWorker _stateQueueWorker;

		public HangfireServerStarter(
			HangfireStarter starter, 
			IConfigReader config,
			StateQueueWorker stateQueueWorker)
		{
			_starter = starter;
			_config = config;
			_stateQueueWorker = stateQueueWorker;
		}

		public void Start(IAppBuilder app)
		{
			_starter.Start(_config.ConnectionString("Hangfire"));
			app.UseHangfireServer(new BackgroundJobServerOptions
			{
				WorkerCount = _config.ReadValue("HangFireWorkerCount", 8),
				Queues = Queues.OrderOfPriority().ToArray()
			}, _stateQueueWorker);
		}
	}
}