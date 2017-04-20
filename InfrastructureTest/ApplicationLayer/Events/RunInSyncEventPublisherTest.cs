using System;
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

			Handler.Succeeded.Should().Be.True();
		}

		[Test]
		public void ShouldRunOnAnotherThread()
		{
			Publisher.Publish(new TestEvent());

			Handler.ThreadId.Should().Not.Be(Thread.CurrentThread.ManagedThreadId);
		}

		[Test]
		public void ShouldRetry()
		{
			Handler.FailOnce = true;
			Publisher.Publish(new TestEvent());
			
			Handler.Succeeded.Should().Be.True();
		}

		[Test]
		public void ShouldTry5Times()
		{
			Handler.Fails = true;
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
			Handler.Fails = true;
			Publisher.Publish(new TestEvent());

			Handler.Attempts.Should().Be(3);
		}

		public class TestEvent : IEvent
		{
		}

		public class Test5AttemptsEvent : IEvent
		{
		}

		public class TestHandler : IHandleEvent<TestEvent>, IHandleEvent<Test5AttemptsEvent>, IRunInSync
		{
			public int ThreadId;
			public bool Succeeded;
			public bool FailOnce;
			public bool Fails;
			public int Attempts;

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

				if (FailOnce)
				{
					FailOnce = false;
					throw new Exception();
				}
				if (Fails)
					throw new Exception();

				ThreadId = Thread.CurrentThread.ManagedThreadId;
				Succeeded = true;
			}

		}
	}
}