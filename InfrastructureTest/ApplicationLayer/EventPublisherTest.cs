﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer
{
	[TestFixture]
	public class EventPublisherTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldInvokeHandler()
		{
			var handler = MockRepository.GenerateMock<IHandleEvent<TestEvent>>();
			var resolver = MockRepository.GenerateMock<IResolve>();
			resolver.Stub(x => x.Resolve(typeof(IEnumerable<IHandleEvent<TestEvent>>))).Return(new[] {handler});
			var target = new EventPublisher(resolver);
			var @event = new TestEvent();

			target.Publish(@event);

			handler.AssertWasCalled(x => x.Handle(@event));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldInvokeMultipleHandlers()
		{
			var handler1 = MockRepository.GenerateMock<IHandleEvent<TestEvent>>();
			var handler2 = MockRepository.GenerateMock<IHandleEvent<TestEvent>>();
			var resolver = MockRepository.GenerateMock<IResolve>();
			resolver.Stub(x => x.Resolve(typeof(IEnumerable<IHandleEvent<TestEvent>>))).Return(new[] { handler1, handler2 });
			var target = new EventPublisher(resolver);
			var @event = new TestEvent();

			target.Publish(@event);

			handler1.AssertWasCalled(x => x.Handle(@event));
			handler2.AssertWasCalled(x => x.Handle(@event));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void ShouldCallCorrectHandleMethod()
		{
			var handler = MockRepository.GenerateMock<ITestHandler>();
			var resolver = MockRepository.GenerateMock<IResolve>();
			resolver.Stub(x => x.Resolve(typeof(IEnumerable<IHandleEvent<TestEventTwo>>))).Return(new[] { handler });
			var target = new EventPublisher(resolver);
			var @event = new TestEventTwo();

			target.Publish(@event);

			handler.AssertWasCalled(x => x.Handle(@event));
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void ShouldSetContextDetails()
        {
            var handler = MockRepository.GenerateMock<IHandleEvent<TestDomainEvent>>();
            var resolver = MockRepository.GenerateMock<IResolve>();
            resolver.Stub(x => x.Resolve(typeof(IEnumerable<IHandleEvent<TestDomainEvent>>))).Return(new[] { handler });
            var target = new EventPublisher(resolver);
            var @event = new TestDomainEvent();

            target.Publish(@event);

            handler.AssertWasCalled(x => x.Handle(@event));
            @event.Datasource.Should().Not.Be.Empty();
            @event.BusinessUnitId.Should().Not.Be.EqualTo(Guid.Empty);
        }
	}
}
