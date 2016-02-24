using System;
using System.Diagnostics;
using System.Threading;
using Contrib.SignalR.SignalRMessageBus;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Infrastructure;

namespace Teleopti.Ccc.Web.Broker
{
	public static class SignalRConfiguration
	{
		public static void Configure(SignalRSettings settingsFromParser, Action mapSignalR)
		{
			if (settingsFromParser.DefaultMessageBufferSize.HasValue)
				GlobalHost.Configuration.DefaultMessageBufferSize = settingsFromParser.DefaultMessageBufferSize.Value;

			if (settingsFromParser.DisconnectTimeout.HasValue)
				GlobalHost.Configuration.DisconnectTimeout = settingsFromParser.DisconnectTimeout.Value;

			if (settingsFromParser.KeepAlive.HasValue)
				GlobalHost.Configuration.KeepAlive = settingsFromParser.KeepAlive.Value;

			if (settingsFromParser.ConnectionTimeout.HasValue)
				GlobalHost.Configuration.ConnectionTimeout = settingsFromParser.ConnectionTimeout.Value;

			switch (settingsFromParser.SignalRBackplaneType)
			{
				case SignalRBackplaneType.SqlServer:
					GlobalHost.DependencyResolver.UseSqlServer(settingsFromParser.SqlServerBackplaneConnectionString);
					break;
				case SignalRBackplaneType.AzureServiceBus:
					GlobalHost.DependencyResolver.UseServiceBus(settingsFromParser.AzureServiceBusBackplaneConnectionString, "Teleopti WFM");
					break;
				default:
					if (settingsFromParser.ScaleOutBackplaneUrl != null)
					{
						GlobalHost.DependencyResolver.UseSignalRServer(settingsFromParser.ScaleOutBackplaneUrl);
					}
					break;
			}

			if (!settingsFromParser.EnablePerformanceCounters)
				GlobalHost.DependencyResolver.Register(typeof(IPerformanceCounterManager), () => new FakePerformanceCounterInitializer());
			
			mapSignalR();

		}
	}

	[CLSCompliant(false)]
	public class FakePerformanceCounterInitializer : IPerformanceCounterManager
	{
		private readonly fakePerformanceCounter _fakePerformanceCounter = new fakePerformanceCounter();

		public FakePerformanceCounterInitializer()
		{
			ConnectionsConnected = _fakePerformanceCounter;
			ConnectionsConnected = _fakePerformanceCounter;
			ConnectionsReconnected = _fakePerformanceCounter;
			ConnectionsDisconnected = _fakePerformanceCounter;
			ConnectionsCurrent = _fakePerformanceCounter;
			ConnectionMessagesReceivedTotal = _fakePerformanceCounter;
			ConnectionMessagesSentTotal = _fakePerformanceCounter;
			ConnectionMessagesReceivedPerSec = _fakePerformanceCounter;
			ConnectionMessagesSentPerSec = _fakePerformanceCounter;
			MessageBusMessagesReceivedTotal = _fakePerformanceCounter;
			MessageBusMessagesReceivedPerSec = _fakePerformanceCounter;
			ScaleoutMessageBusMessagesReceivedPerSec = _fakePerformanceCounter;
			MessageBusMessagesPublishedTotal = _fakePerformanceCounter;
			MessageBusMessagesPublishedPerSec = _fakePerformanceCounter;
			MessageBusSubscribersCurrent = _fakePerformanceCounter;
			MessageBusSubscribersTotal = _fakePerformanceCounter;
			MessageBusSubscribersPerSec = _fakePerformanceCounter;
			MessageBusAllocatedWorkers = _fakePerformanceCounter;
			MessageBusBusyWorkers = _fakePerformanceCounter;
			MessageBusTopicsCurrent = _fakePerformanceCounter;
			ErrorsAllTotal = _fakePerformanceCounter;
			ErrorsAllPerSec = _fakePerformanceCounter;
			ErrorsHubResolutionTotal = _fakePerformanceCounter;
			ErrorsHubResolutionPerSec = _fakePerformanceCounter;
			ErrorsHubInvocationTotal = _fakePerformanceCounter;
			ErrorsHubInvocationPerSec = _fakePerformanceCounter;
			ErrorsTransportTotal = _fakePerformanceCounter;
			ErrorsTransportPerSec = _fakePerformanceCounter;
			ScaleoutStreamCountTotal = _fakePerformanceCounter;
			ScaleoutStreamCountOpen = _fakePerformanceCounter;
			ScaleoutStreamCountBuffering = _fakePerformanceCounter;
			ScaleoutErrorsTotal = _fakePerformanceCounter;
			ScaleoutErrorsPerSec = _fakePerformanceCounter;
			ScaleoutSendQueueLength = _fakePerformanceCounter;
		    ConnectionsCurrentForeverFrame = _fakePerformanceCounter;
		    ConnectionsCurrentLongPolling = _fakePerformanceCounter;
		    ConnectionsCurrentServerSentEvents = _fakePerformanceCounter;
		    ConnectionsCurrentWebSockets = _fakePerformanceCounter;
		}

		public void Initialize(string instanceName, CancellationToken hostShutdownToken)
		{
		}

		public IPerformanceCounter LoadCounter(string categoryName, string counterName, string instanceName, bool isReadOnly)
		{
			return _fakePerformanceCounter;
		}

		private class fakePerformanceCounter : IPerformanceCounter
		{
			public long Decrement()
			{
				return 0;
			}

			public long Increment()
			{
				return 0;
			}

			public long IncrementBy(long value)
			{
				return value;
			}

			public CounterSample NextSample()
			{
				return new CounterSample();
			}

			public void Close()
			{
			}

			public void RemoveInstance()
			{
			}

			public string CounterName { get; private set; }
			public long RawValue { get; set; }
		}

		public IPerformanceCounter ConnectionsConnected { get; private set; }
		public IPerformanceCounter ConnectionsReconnected { get; private set; }
		public IPerformanceCounter ConnectionsDisconnected { get; private set; }
	    public IPerformanceCounter ConnectionsCurrentForeverFrame { get; private set; }
	    public IPerformanceCounter ConnectionsCurrentLongPolling { get; private set; }
	    public IPerformanceCounter ConnectionsCurrentServerSentEvents { get; private set; }
	    public IPerformanceCounter ConnectionsCurrentWebSockets { get; private set; }
	    public IPerformanceCounter ConnectionsCurrent { get; private set; }
		public IPerformanceCounter ConnectionMessagesReceivedTotal { get; private set; }
		public IPerformanceCounter ConnectionMessagesSentTotal { get; private set; }
		public IPerformanceCounter ConnectionMessagesReceivedPerSec { get; private set; }
		public IPerformanceCounter ConnectionMessagesSentPerSec { get; private set; }
		public IPerformanceCounter MessageBusMessagesReceivedTotal { get; private set; }
		public IPerformanceCounter MessageBusMessagesReceivedPerSec { get; private set; }
		public IPerformanceCounter ScaleoutMessageBusMessagesReceivedPerSec { get; private set; }
		public IPerformanceCounter MessageBusMessagesPublishedTotal { get; private set; }
		public IPerformanceCounter MessageBusMessagesPublishedPerSec { get; private set; }
		public IPerformanceCounter MessageBusSubscribersCurrent { get; private set; }
		public IPerformanceCounter MessageBusSubscribersTotal { get; private set; }
		public IPerformanceCounter MessageBusSubscribersPerSec { get; private set; }
		public IPerformanceCounter MessageBusAllocatedWorkers { get; private set; }
		public IPerformanceCounter MessageBusBusyWorkers { get; private set; }
		public IPerformanceCounter MessageBusTopicsCurrent { get; private set; }
		public IPerformanceCounter ErrorsAllTotal { get; private set; }
		public IPerformanceCounter ErrorsAllPerSec { get; private set; }
		public IPerformanceCounter ErrorsHubResolutionTotal { get; private set; }
		public IPerformanceCounter ErrorsHubResolutionPerSec { get; private set; }
		public IPerformanceCounter ErrorsHubInvocationTotal { get; private set; }
		public IPerformanceCounter ErrorsHubInvocationPerSec { get; private set; }
		public IPerformanceCounter ErrorsTransportTotal { get; private set; }
		public IPerformanceCounter ErrorsTransportPerSec { get; private set; }
		public IPerformanceCounter ScaleoutStreamCountTotal { get; private set; }
		public IPerformanceCounter ScaleoutStreamCountOpen { get; private set; }
		public IPerformanceCounter ScaleoutStreamCountBuffering { get; private set; }
		public IPerformanceCounter ScaleoutErrorsTotal { get; private set; }
		public IPerformanceCounter ScaleoutErrorsPerSec { get; private set; }
		public IPerformanceCounter ScaleoutSendQueueLength { get; private set; }
	}
}