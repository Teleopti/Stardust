using Microsoft.Azure.ServiceBus;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public interface IServiceBusTopicClientFactory
	{
		ITopicClient CreateTopicClient(string serviceBusAddress, string topicName);
	}
}