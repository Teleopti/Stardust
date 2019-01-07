using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class TriggerInsightsDataRefreshJobStep : JobStepBase
	{
		private readonly IServiceBusTopicClientFactory _serviceBusTopicClientProvider;

		public TriggerInsightsDataRefreshJobStep(IJobParameters jobParameters,
			IServiceBusTopicClientFactory serviceBusTopicClientProvider)
			: base(jobParameters)
		{
			_serviceBusTopicClientProvider = serviceBusTopicClientProvider;
			Name = "Trigger Insights data refresh";
			IsBusinessUnitIndependent = true;
		}

		protected override int RunStep(IList<IJobResult> jobResultCollection, bool isLastBusinessUnit)
		{
			sendMessagesAsync().GetAwaiter().GetResult();
			return 0;
		}

		private async Task sendMessagesAsync()
		{
			ITopicClient topicClient = null;

			var insightsConfig = _jobParameters.InsightsConfig;
			try
			{
				topicClient = _serviceBusTopicClientProvider.CreateTopicClient(insightsConfig.ServiceBusAddress,
						insightsConfig.TopicName);
				var startRefreshMsg = new StartRefreshMessage
				{
					ModelLocation = insightsConfig.ModelLocation,
					IsFullRefresh = false,
					ModelName = insightsConfig.ModelName,
					AnalyticsDatabase = insightsConfig.AnalyticsDatabase,
					AnalysisService = insightsConfig.AnalysisService,
					Location = insightsConfig.Location
				};

				var startRefreshMsgJson = JsonConvert.SerializeObject(startRefreshMsg);
				var startRefreshMsgByteArray =
					new MemoryStream(Encoding.UTF8.GetBytes(startRefreshMsgJson ?? "")).ToArray();

				var message = new Message(startRefreshMsgByteArray);
				await topicClient.SendAsync(message);
			}
			finally
			{
				topicClient?.CloseAsync();
			}
		}
	}
}