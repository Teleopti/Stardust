﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[TestFixture]
	public class EventPublisherTest
	{
		[Test]
		public void ShouldInvokeHandler()
		{
			var handler = MockRepository.GenerateMock<IHandleEvent<TestEvent>>();
			var resolver = MockRepository.GenerateMock<IResolve>();
			resolver.Stub(x => x.Resolve(typeof(IEnumerable<IHandleEvent<TestEvent>>))).Return(new[] { handler });
			var target = new SyncEventPublisher(resolver);
			var @event = new TestEvent();

			target.Publish(@event);

			handler.AssertWasCalled(x => x.Handle(@event));
		}

		[Test]
		public void ShouldInvokeMultipleHandlers()
		{
			var handler1 = MockRepository.GenerateMock<IHandleEvent<TestEvent>>();
			var handler2 = MockRepository.GenerateMock<IHandleEvent<TestEvent>>();
			var resolver = MockRepository.GenerateMock<IResolve>();
			resolver.Stub(x => x.Resolve(typeof(IEnumerable<IHandleEvent<TestEvent>>))).Return(new[] { handler1, handler2 });
			var target = new SyncEventPublisher(resolver);
			var @event = new TestEvent();

			target.Publish(@event);

			handler1.AssertWasCalled(x => x.Handle(@event));
			handler2.AssertWasCalled(x => x.Handle(@event));
		}

		[Test]
		public void ShouldCallCorrectHandleMethod()
		{
			var handler = MockRepository.GenerateMock<ITestHandler>();
			var resolver = MockRepository.GenerateMock<IResolve>();
			resolver.Stub(x => x.Resolve(typeof(IEnumerable<IHandleEvent<TestEventTwo>>))).Return(new[] { handler });
			var target = new SyncEventPublisher(resolver);
			var @event = new TestEventTwo();

			target.Publish(@event);

			handler.AssertWasCalled(x => x.Handle(@event));
		}

		[Test]
		public void ShouldSetContextDetails()
		{
			var handler = MockRepository.GenerateMock<IHandleEvent<TestDomainEvent>>();
			var resolver = MockRepository.GenerateMock<IResolve>();
			resolver.Stub(x => x.Resolve(typeof(IEnumerable<IHandleEvent<TestDomainEvent>>))).Return(new[] { handler });
			var target = new EventPopulatingPublisher(new SyncEventPublisher(resolver), new EventContextPopulator(new CurrentBusinessUnit(new CurrentIdentity(new CurrentTeleoptiPrincipal())), new CurrentDataSource(new CurrentIdentity(new CurrentTeleoptiPrincipal()), null, null), new FakeCurrentInitiatorIdentifier()));
			var @event = new TestDomainEvent();

			target.Publish(@event);

			handler.AssertWasCalled(x => x.Handle(@event));
			@event.Datasource.Should().Not.Be.Empty();
			@event.BusinessUnitId.Should().Not.Be.EqualTo(Guid.Empty);
		}

		public class TestEvent : Event
		{
		}

		public class TestEventTwo : Event
		{
		}

		public class TestDomainEvent : EventWithLogOnAndInitiator
		{
		}

		public interface ITestHandler : IHandleEvent<TestEvent>, IHandleEvent<TestEventTwo>
		{
		}

	}

}
