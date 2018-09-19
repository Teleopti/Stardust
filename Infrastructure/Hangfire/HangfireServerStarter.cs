using System.Linq;
using Hangfire;
using Owin;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.RealTimeAdherence.Domain;
using Teleopti.Ccc.Infrastructure.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Infrastructure.RealTimeAdherence.Tracer;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireServerStarter
	{
		private readonly HangfireStarter _starter;
		private readonly IConfigReader _config;
		private readonly StateQueueWorker _stateQueueWorker;
		private readonly RtaTracerRefresher _rtaTracerRefresher;
		private readonly RtaEventStoreSynchronizerProcess _eventStoreSynchronizerProcess;
		private readonly RecurringEventPublishingUpdater _recurringEventPublishingUpdater;

		public HangfireServerStarter(
			HangfireStarter starter,
			IConfigReader config,
			StateQueueWorker stateQueueWorker,
			RtaTracerRefresher rtaTracerRefresher,
			RtaEventStoreSynchronizerProcess eventStoreSynchronizerProcess,
			RecurringEventPublishingUpdater recurringEventPublishingUpdater)
		{
			_starter = starter;
			_config = config;
			_stateQueueWorker = stateQueueWorker;
			_rtaTracerRefresher = rtaTracerRefresher;
			_eventStoreSynchronizerProcess = eventStoreSynchronizerProcess;
			_recurringEventPublishingUpdater = recurringEventPublishingUpdater;
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
				_stateQueueWorker,
				_rtaTracerRefresher,
				_eventStoreSynchronizerProcess,
				_recurringEventPublishingUpdater
			);
		}
	}
}