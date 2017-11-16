using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Hangfire;
using Hangfire.Server;
using log4net;
using Newtonsoft.Json;
using Owin;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages;
using Teleopti.Ccc.Infrastructure.Rta;

namespace Teleopti.Ccc.Infrastructure.Hangfire
{
	public class HangfireServerStarter
	{
		private readonly HangfireStarter _starter;
		private readonly IConfigReader _config;
		private readonly StateQueueWorker _stateQueueWorker;
		private readonly RtaTracerRefresher _rtaTracerRefresher;
		private readonly NewSuperDuperIntegrationManager _superDuper;

		public HangfireServerStarter(
			HangfireStarter starter,
			IConfigReader config,
			StateQueueWorker stateQueueWorker,
			RtaTracerRefresher rtaTracerRefresher,
			NewSuperDuperIntegrationManager superDuper)
		{
			_starter = starter;
			_config = config;
			_stateQueueWorker = stateQueueWorker;
			_rtaTracerRefresher = rtaTracerRefresher;
			_superDuper = superDuper;
		}

		public void Start(IAppBuilder app)
		{
			_starter.Start(_config.ConnectionString("Hangfire"));
			app.UseHangfireServer(new BackgroundJobServerOptions
				{
					WorkerCount = _config.ReadValue("HangFireWorkerCount", 8),
					Queues = Queues.OrderOfPriority().ToArray()
				},
				_stateQueueWorker,
				_rtaTracerRefresher,
				_superDuper
			);
		}
	}

	public class NewSuperDuperIntegrationManager : IBackgroundProcess
	{
		private readonly Domain.ApplicationLayer.Rta.Service.Rta _rta;
		private ILog log = LogManager.GetLogger(typeof(NewSuperDuperIntegrationManager));

		public NewSuperDuperIntegrationManager(Domain.ApplicationLayer.Rta.Service.Rta rta)
		{
			_rta = rta;
		}

		public void Execute(BackgroundProcessContext context)
		{
			
			var baseUrl = "https://vcc-na12b.8x8.com/api/sapi/atmosphere/subscribe/TenantUpdates";
			var tenantId = "a8x8inccss01";
			var subsId = "mattias";
			var token = "9889132679ca905b10ddffdce8a959b2";

			var httpClientHandler = new HttpClientHandler {Credentials = new NetworkCredential(tenantId, token)};
			var httpClient = new HttpClient(httpClientHandler);

			var currentTimestamp = Math.Floor((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds);

			var url = baseUrl + "-" + tenantId + "-" + subsId + "?" + "tenantId=" + tenantId + "&subsId=" + subsId +
					  "&desiredOutputType=JSON&_=" + currentTimestamp;
			

			httpClient.Timeout = Timeout.InfiniteTimeSpan;

			using (var stream = httpClient.GetStreamAsync(url).Result)
			{
				var serializer = new JsonSerializer();
				using (var reader = new StreamReader(stream))
				{
					using (var jsonReader = new JsonTextReader(reader) {SupportMultipleContent = true})
					{
						while (jsonReader.Read())
						{
							var parsed = serializer.Deserialize<parsed>(jsonReader);
							if (parsed.AgentStatusChange != null)
							{
								var state = parsed.AgentStatusChange;

								_rta.Enqueue(new BatchInputModel
								{
									AuthenticationKey = "!#¤atAbgT%",
									SourceId = "1",
									States = new[]
									{
										new BatchStateInputModel
										{
											UserCode = state.agentId,
											StateCode = state.newState,
											StateDescription = state.newReasonCodeUser
										}
									}
								});

								log.Warn(JsonConvert.SerializeObject(parsed.AgentStatusChange));
								
							}
						}
					}
				}
			}
		}

		public class parsed
		{
			public AgentStatusChange AgentStatusChange;
		}

		public class AgentStatusChange
		{
			public string agentId;
			public string newState;
			public string newReasonCodeUser;

			public string newSubState;
			public string newSubStateReason;
			public string statusEventTS;
		}
	}
}