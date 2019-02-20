using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Steps;
using Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.Steps
{
	[TestFixture]
	public class TriggerInsightsDataRefreshJobStepTest
	{
		[Test]
		public void ShouldSendTriggerDataRefreshMessage()
		{
			var jobParameters = JobParametersFactory.SimpleParametersWithInsightsFlag(true);
			var topicClientFactory = new FakeTopicClientFactory();

			var etlConfiguration = new BaseConfiguration(1052, 15, "Mountain Standard Time", false)
			{
				InsightsConfig =
				{
					AnalysisService = "TestAnalysisService",
					AnalyticsDatabase = "TestAnalyticsDatabase",
					Location = "TestLocation",
					ModelLocation = "TestModelLocation",
					ModelName = "TestModelName",
					ServiceBusAddress = "TestServiceBusAddress",
					TopicName = "TestTopicName"
				}
			};

			jobParameters.SetTenantBaseConfigValues(etlConfiguration);

			var target = new TriggerInsightsDataRefreshJobStep(jobParameters, topicClientFactory);
			target.Run(new List<IJobStep>(), null, new List<IJobResult>(), false);

			var topicClient = (FakeTopicClient) topicClientFactory.TopicClient;
			Assert.AreEqual(topicClient.ServiceBusConnectionString, etlConfiguration.InsightsConfig.ServiceBusAddress);
			Assert.AreEqual(topicClient.TopicName, etlConfiguration.InsightsConfig.TopicName);

			var serviceBusMessage = topicClient.Messages.Single();
			
			var jsonStr = Encoding.UTF8.GetString(serviceBusMessage.Body);
			var message = JsonConvert.DeserializeObject<StartRefreshMessage>(jsonStr);
			Assert.AreEqual(message.ModelLocation, etlConfiguration.InsightsConfig.ModelLocation);
			Assert.AreEqual(message.AnalysisService, etlConfiguration.InsightsConfig.AnalysisService);
			Assert.AreEqual(message.AnalyticsDatabase, etlConfiguration.InsightsConfig.AnalyticsDatabase);
			Assert.AreEqual(message.IsFullRefresh, false);
			Assert.AreEqual(message.Location, etlConfiguration.InsightsConfig.Location);
			Assert.AreEqual(message.ModelName, etlConfiguration.InsightsConfig.ModelName);
		}

		[Test]
		public void ShouldDoNothingIfInsightsConfigurationIsInvalid_1()
		{
			var jobParameters = JobParametersFactory.SimpleParametersWithInsightsFlag(true);
			var topicClientFactory = new FakeTopicClientFactory();

			var etlConfiguration = new BaseConfiguration(1052, 15, "Mountain Standard Time", false);

			jobParameters.SetTenantBaseConfigValues(etlConfiguration);

			var target = new TriggerInsightsDataRefreshJobStep(jobParameters, topicClientFactory);
			target.Run(new List<IJobStep>(), null, new List<IJobResult>(), false);

			topicClientFactory.TopicClient.Should().Be.Null();
		}

		[Test]
		public void ShouldDoNothingIfInsightsConfigurationIsInvalid_2()
		{
			var jobParameters = JobParametersFactory.SimpleParametersWithInsightsFlag(true);
			var topicClientFactory = new FakeTopicClientFactory();

			var etlConfiguration = new BaseConfiguration(1052, 15, "Mountain Standard Time", false)
			{
				InsightsConfig =
				{
					AnalysisService = null,
					AnalyticsDatabase = "",
					Location = "TestLocation",
					ModelLocation = "TestModelLocation",
					ModelName = "TestModelName",
					ServiceBusAddress = "TestServiceBusAddress",
					TopicName = "TestTopicName"
				}
			};

			jobParameters.SetTenantBaseConfigValues(etlConfiguration);

			var target = new TriggerInsightsDataRefreshJobStep(jobParameters, topicClientFactory);
			target.Run(new List<IJobStep>(), null, new List<IJobResult>(), false);

			topicClientFactory.TopicClient.Should().Be.Null();
		}

		[Test]
		public void ShouldDoNothingIfInsightsConfigurationIsInvalid_3()
		{
			var jobParameters = JobParametersFactory.SimpleParametersWithInsightsFlag(false);
			var topicClientFactory = new FakeTopicClientFactory();

			var etlConfiguration = new BaseConfiguration(1052, 15, "Mountain Standard Time", false)
			{
				InsightsConfig =
				{
					AnalysisService = "TestAnalysisService",
					AnalyticsDatabase = "TestAnalyticsDatabase",
					Location = "TestLocation",
					ModelLocation = "TestModelLocation",
					ModelName = "TestModelName",
					ServiceBusAddress = "TestServiceBusAddress",
					TopicName = "TestTopicName"
				}
			};

			jobParameters.SetTenantBaseConfigValues(etlConfiguration);

			var target = new TriggerInsightsDataRefreshJobStep(jobParameters, topicClientFactory);
			target.Run(new List<IJobStep>(), null, new List<IJobResult>(), false);

			topicClientFactory.TopicClient.Should().Be.Null();
		}
	}
}