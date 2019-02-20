using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;

namespace Teleopti.Analytics.Etl.Common.Transformer.Job.Steps
{
	public class TriggerInsightsDataRefreshJobStep : JobStepBase
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(TriggerInsightsDataRefreshJobStep));
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
			var insightsConfig = _jobParameters.InsightsConfig;
			if (!_jobParameters.InsightsEnabled || insightsConfig == null || !insightsConfig.IsValid())
			{
				// It's not an problem (When scheduled Insights ETL job for all tenants, some tenant is not
				// enabled with Insights and no configuration applied, then no message should be send out for them),
				logger.InfoFormat(
					"Insights not enabled or configuration for Insights ETL job is invalid for data source {0}.",
					_jobParameters.Helper.SelectedDataSource.DataSourceName);
				return;
			}

			ITopicClient topicClient = null;
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

				logger.Debug($"Sent message to ServiceBus with Address: \"{insightsConfig.ServiceBusAddress}\" and "
							  + $"TopicName: \"{insightsConfig.TopicName}\":{Environment.NewLine}{startRefreshMsgJson}");
			}
			catch (Exception ex)
			{
				logger.Error("Exception sending Refresh message.", ex);
				throw;
			}
			finally
			{
				topicClient?.CloseAsync();
				logger.Debug("topicClient closed successfully");
			}
		}
	}
}
