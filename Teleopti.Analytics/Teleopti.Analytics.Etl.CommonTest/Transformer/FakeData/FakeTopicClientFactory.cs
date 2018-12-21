using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Teleopti.Analytics.Etl.Common.Infrastructure;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.FakeData
{
	public class FakeTopicClient : ITopicClient
	{
		public string ServiceBusConnectionString { get; private set; }
		public List<Message> Messages;

		public FakeTopicClient(string serviceBusAddress, string topicName)
		{
			ServiceBusConnectionString = serviceBusAddress;
			TopicName = topicName;
			Messages = new List<Message>();
		}

		public Task CloseAsync()
		{
			return Task.CompletedTask;
		}

		public void RegisterPlugin(ServiceBusPlugin serviceBusPlugin)
		{
			throw new NotImplementedException();
		}

		public void UnregisterPlugin(string serviceBusPluginName)
		{
			throw new NotImplementedException();
		}

		public string ClientId { get; }
		public bool IsClosedOrClosing { get; }
		public string Path { get; }
		public TimeSpan OperationTimeout { get; set; }
		public ServiceBusConnection ServiceBusConnection { get; }
		public IList<ServiceBusPlugin> RegisteredPlugins { get; }
		public Task SendAsync(Message message)
		{
			Messages.Add(message);
			return Task.CompletedTask;
		}

		public Task SendAsync(IList<Message> messageList)
		{
			Messages.AddRange(messageList);
			return Task.CompletedTask;
		}

		public Task<long> ScheduleMessageAsync(Message message, DateTimeOffset scheduleEnqueueTimeUtc)
		{
			throw new NotImplementedException();
		}

		public Task CancelScheduledMessageAsync(long sequenceNumber)
		{
			throw new NotImplementedException();
		}

		public string TopicName { get; }
	}

	public class FakeTopicClientFactory : IServiceBusTopicClientFactory
	{
		public ITopicClient TopicClient { get; private set; }

		public ITopicClient CreateTopicClient(string serviceBusAddress, string topicName)
		{
			TopicClient = new FakeTopicClient(serviceBusAddress, topicName);
			return TopicClient;
		}
	}
}