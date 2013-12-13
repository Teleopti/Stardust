﻿using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.ServiceBus;
using Rhino.ServiceBus.Impl;
using Rhino.ServiceBus.Internal;
using Rhino.ServiceBus.Messages;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
	[TestFixture]
	public class LocalServiceBusPublisherTest
	{
		[Test]
		public void ShouldPopulateEventWithInitiatorId()
		{
			var bus = new FakeServiceBus();
			var @event = new AnEventThatCanHaveInitiatorId();
			var initiatorId = Guid.NewGuid();
			var target = new LocalServiceBusPublisher(
				bus,
				new EventContextPopulator(null,
					new FakeCurrentInitiatorIdentifier(
						new FakeInitiatorIdentifier { InitiatorId = initiatorId })));

			target.Publish(@event);

			var sent = bus.Sent as AnEventThatCanHaveInitiatorId;
			sent.InitiatorId.Should().Be(initiatorId);
		}
	}

	public class AnEventThatCanHaveInitiatorId : RaptorDomainEvent
	{
	}

	public class FakeServiceBus : IServiceBus
	{
		public object Sent { get; set; }

		public void Publish(params object[] messages)
		{
			throw new NotImplementedException();
		}

		public void Notify(params object[] messages)
		{
			throw new NotImplementedException();
		}

		public void Reply(params object[] messages)
		{
			throw new NotImplementedException();
		}

		public void Send(Endpoint endpoint, params object[] messages)
		{
			throw new NotImplementedException();
		}

		public void Send(params object[] messages)
		{
			Sent = messages.Last();
		}

		public void ConsumeMessages(params object[] messages)
		{
			throw new NotImplementedException();
		}

		public IDisposable AddInstanceSubscription(IMessageConsumer consumer)
		{
			throw new NotImplementedException();
		}

		public void Subscribe<T>()
		{
			throw new NotImplementedException();
		}

		public void Subscribe(Type type)
		{
			throw new NotImplementedException();
		}

		public void Unsubscribe<T>()
		{
			throw new NotImplementedException();
		}

		public void Unsubscribe(Type type)
		{
			throw new NotImplementedException();
		}

		public void DelaySend(Endpoint endpoint, DateTime time, params object[] msgs)
		{
			throw new NotImplementedException();
		}

		public void DelaySend(DateTime time, params object[] msgs)
		{
			throw new NotImplementedException();
		}

		public Endpoint Endpoint { get; private set; }
		public CurrentMessageInformation CurrentMessageInformation { get; private set; }
		public event Action<Reroute> ReroutedEndpoint;
	}
}