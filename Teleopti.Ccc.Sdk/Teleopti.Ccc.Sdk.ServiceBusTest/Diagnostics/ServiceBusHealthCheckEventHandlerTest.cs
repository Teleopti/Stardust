using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Diagnostics
{
	[TestFixture]
	public class ServiceBusHealthCheckEventHandlerTest
	{
		public ServiceBusHealthCheckEventHandler Target;
		public FakeMessageBrokerComposite Sender;
		public FakeDeviceInfoProvider DeviceInfo;

		[SetUp]
		public void Setup()
		{
			Sender = new FakeMessageBrokerComposite();
			DeviceInfo = new FakeDeviceInfoProvider();
			Target = new ServiceBusHealthCheckEventHandler(Sender, DeviceInfo);
		}

		[Test]
		public void ShouldHandleEvent()
		{
			Target.Handle(new ServiceBusHealthCheckEvent());

			Sender.SentCount().Should().Be(1);
		}

		[Test]
		public void ShouldSendDiagnosticsInfo()
		{
			Target.Handle(new ServiceBusHealthCheckEvent());

			Sender.SentMessageType().Should().Be(typeof(ITeleoptiDiagnosticsInformation));
		}
	}

	public class FakeMessageBrokerComposite : IMessageBrokerComposite
	{
		private int _sentCount;
		private Type _messageType;
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId,
			Guid referenceObjectId, Type referenceObjectType, Guid domainObjectId, Type domainObjectType,
			DomainUpdateType updateType, byte[] domainObject, Guid? trackId = null)
		{
			throw new NotImplementedException();
		}

		public void Send(string dataSource, Guid businessUnitId, DateTime eventStartDate, DateTime eventEndDate, Guid moduleId,
			Guid domainObjectId, Type domainObjectType, DomainUpdateType updateType, byte[] domainObject)
		{
			_sentCount++;
			_messageType = domainObjectType;
		}

		public void Send(string dataSource, Guid businessUnitId, IEventMessage[] eventMessages)
		{
			throw new NotImplementedException();
		}

		public void RegisterSubscription(Subscription subscription, EventHandler<EventMessageArgs> eventMessageHandler)
		{
			throw new NotImplementedException();
		}

		public void UnregisterSubscription(EventHandler<EventMessageArgs> eventMessageHandler)
		{
			throw new NotImplementedException();
		}

		public void Send(Message message)
		{
			throw new NotImplementedException();
		}

		public void SendMultiple(IEnumerable<Message> messages)
		{
			throw new NotImplementedException();
		}

		public bool IsAlive { get; }
		public bool IsPollingAlive { get; }
		public string ServerUrl { get; set; }
		public void StartBrokerService(bool useLongPolling = false)
		{
			throw new NotImplementedException();
		}

		public int SentCount()
		{
			return _sentCount;
		}
		public Type SentMessageType()
		{
			return _messageType;
		}
	}

	public class FakeDeviceInfoProvider : IDeviceInfoProvider
	{
		public DeviceInfo GetDeviceInfo()
		{
			return new DeviceInfo();
		}
	}
}
