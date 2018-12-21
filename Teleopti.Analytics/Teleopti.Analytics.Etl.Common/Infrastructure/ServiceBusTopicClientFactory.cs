using Microsoft.Azure.ServiceBus;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public class ServiceBusTopicClientFactory: IServiceBusTopicClientFactory
	{
		public ITopicClient CreateTopicClient(string serviceBusAddress, string topicName)
		{
			return new TopicClient(serviceBusAddress, topicName);
		}
	}
}
