using System;
using Autofac;
using NUnit.Framework;
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

		public IResolve ResolverWith(Type type, object instance)
		{
			var builder = new ContainerBuilder();
			builder.RegisterInstance(instance).As(type);
			return new AutofacResolve(builder.Build());
		}

		[Test]
		public void ShouldInvokeHandler()
		{
			var handler = new FakeHandler();
			var target = new SyncEventPublisher(ResolverWith(typeof(IHandleEvent<TestEvent>), handler));
			var @event = new TestEvent();

			target.Publish(@event);

			handler.CalledWithEvent.Should().Be(@event);
		}

		[Test]
		public void ShouldInvokeMultipleHandlers()
		{
			var handler1 = new FakeHandler();
			var handler2 = new FakeHandler();
			var builder = new ContainerBuilder();
			builder.RegisterInstance(handler1).As<IHandleEvent<TestEvent>>();
			builder.RegisterInstance(handler2).As<IHandleEvent<TestEvent>>();
			var target = new SyncEventPublisher(new AutofacResolve(builder.Build()));
			var @event = new TestEvent();

			target.Publish(@event);

			handler1.CalledWithEvent.Should().Be(@event);
			handler2.CalledWithEvent.Should().Be(@event);
		}

		[Test]
		public void ShouldCallCorrectHandleMethod()
		{
			var handler = new FakeHandler();
			var target = new SyncEventPublisher(ResolverWith(typeof(IHandleEvent<TestEventTwo>), handler));
			var @event = new TestEventTwo();

			target.Publish(@event);

			handler.CalledWithEventTwo.Should().Be(@event);
		}

		[Test]
		public void ShouldSetContextDetails()
		{
			var handler = new FakeHandler();
			var target = new EventPopulatingPublisher(new SyncEventPublisher(ResolverWith(typeof(IHandleEvent<TestDomainEvent>), handler)), new EventContextPopulator(new CurrentBusinessUnit(new CurrentIdentity(new CurrentTeleoptiPrincipal())), new CurrentDataSource(new CurrentIdentity(new CurrentTeleoptiPrincipal()), null, null), new FakeCurrentInitiatorIdentifier()));
			var @event = new TestDomainEvent();

			target.Publish(@event);

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

		public class FakeHandler : IHandleEvent<TestEvent>, IHandleEvent<TestEventTwo>, IHandleEvent<TestDomainEvent>
		{
			public Event CalledWithEvent;
			public Event CalledWithEventTwo;

			public void Handle(TestEvent @event)
			{
				CalledWithEvent = @event;
			}

			public void Handle(TestEventTwo @event)
			{
				CalledWithEventTwo = @event;
			}

			public void Handle(TestDomainEvent @event)
			{
			}
		}
	}

}
