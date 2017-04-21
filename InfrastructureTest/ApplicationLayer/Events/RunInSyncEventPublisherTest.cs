using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.ApplicationLayer.Events
{
	[InfrastructureTest]
	public class RunInSyncEventPublisherTest : ISetup
	{
		public IEventPublisher Publisher;
		public TestHandler Handler;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<TestHandler>();
		}

		[Test]
		public void ShouldInvokeHandler()
		{
			Publisher.Publish(new TestEvent());

			Handler.Attempts.Should().Be(1);
			Handler.Succeeded.Should().Be.True();
		}

		[Test]
		public void ShouldRunOnAnotherThread()
		{
			Publisher.Publish(new TestEvent());

			Handler.ThreadIds.Single().Should().Not.Be(Thread.CurrentThread.ManagedThreadId);
		}

		[Test]
		public void ShouldRetry()
		{
			Handler.Fails(1, new EventPublisherException());
			Publisher.Publish(new TestEvent());
			
			Handler.Succeeded.Should().Be.True();
		}

		[Test]
		public void ShouldTry5Times()
		{
			Handler.Fails(5, new EventPublisherException());
			Publisher.Publish(new Test5AttemptsEvent());

			Handler.Attempts.Should().Be(5);
		}

		[Test]
		public void ShouldNotRetryOnSuccess()
		{
			Publisher.Publish(new TestEvent());

			Handler.Attempts.Should().Be(1);
		}

		[Test]
		public void ShouldTry3TimesByDefault()
		{
			Handler.Fails(3, new EventPublisherException());
			Publisher.Publish(new TestEvent());

			Handler.Attempts.Should().Be(3);
		}

		[Test]
		public void ShouldRunEachAttemptOnANewThread()
		{
			Handler.Fails(10, new EventPublisherException());

			Publisher.Publish(new TestEvent());

			Handler.ThreadIds.Distinct().Should().Have.SameSequenceAs(Handler.ThreadIds);
		}

		[Test]
		[Setting("BehaviorTestServer", false)]
		public void ShouldLogExceptionsInProduction()
		{
			Handler.Fails(10, new EventPublisherException());

			Assert.DoesNotThrow(() =>
			{
				Publisher.Publish(new TestEvent());
			});
		}

		[Test]
		[Setting("BehaviorTestServer", true)]
		public void ShouldRethrowAllExceptionsWhenTesting()
		{
			Handler.Fails(10, new EventPublisherException());

			var exception = Assert.Throws<AggregateException>(() =>
			{
				Publisher.Publish(new TestEvent());
			});
			exception.InnerExceptions.Count.Should().Be(Handler.Attempts);
		}
		
		public class TestEvent : IEvent
		{
		}

		public class Test5AttemptsEvent : IEvent
		{
		}

		public class TestHandler : IHandleEvent<TestEvent>, IHandleEvent<Test5AttemptsEvent>, IRunInSync
		{
			private int _fails;
			private Exception _exception;

			public List<int> ThreadIds = new List<int>();
			public bool Succeeded;
			public int Attempts;

			public void Fails(int amountOfFails, Exception exception)
			{
				_fails = amountOfFails;
				_exception = exception;
			}

			public void Handle(TestEvent @event)
			{
				handle();
			}

			[Attempts(5)]
			public void Handle(Test5AttemptsEvent @event)
			{
				handle();
			}

			private void handle()
			{
				Attempts += 1;
				ThreadIds.Add(Thread.CurrentThread.ManagedThreadId);

				if (_fails - Attempts > 0)
					throw _exception;

				Succeeded = true;
			}

		}

		public class EventPublisherException : Exception
		{
		}
	}
}