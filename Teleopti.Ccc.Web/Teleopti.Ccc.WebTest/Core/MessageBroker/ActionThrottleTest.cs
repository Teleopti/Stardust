using System;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Web.Broker;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker
{
	[TestFixture]
	public class ActionThrottleTest
	{
		[Test]
		public void ShouldNotExecuteActionUntilStarted()
		{
			var executed = false;
			var target = new ActionThrottle(1000);

			target.Do(() => { executed = true; });
			Thread.Sleep(100);

			executed.Should().Be.False();
		}

		[Test]
		public void ShouldEventuallyExecuteActions()
		{
			var executed = 0;
			var target = new ActionThrottle(1000);

			target.Do(() => { executed++; });
			target.Do(() => { executed++; });
			target.Start();

			Assert.That(() => executed, Is.EqualTo(2).After(1000, 1));
		}

		[Test]
		public void ShouldExecuteActionsOnAnotherThread()
		{
			var threadId1 = 0;
			var threadId2 = 0;
			var target = new ActionThrottle(1000);

			target.Do(() => { threadId1 = Thread.CurrentThread.ManagedThreadId; });
			target.Do(() => { threadId2 = Thread.CurrentThread.ManagedThreadId; });
			target.Start();

			Assert.That(() => threadId1, Is.Not.EqualTo(0).After(1000, 1));
			threadId1.Should().Not.Be(Thread.CurrentThread.ManagedThreadId);
			Assert.That(() => threadId2, Is.Not.EqualTo(0).After(1000, 1));
			threadId2.Should().Be(threadId1);
		}

		[Test]
		public void ShouldAcceptThrottleArgument()
		{
			var time1 = DateTime.MinValue;
			var time2 = DateTime.MinValue;
			var target = new ActionThrottle(1);

			target.Do(() => { time1 = DateTime.Now; });
			target.Do(() => { time2 = DateTime.Now; });
			target.Start();

			Assert.That(() => time2, Is.Not.EqualTo(DateTime.MinValue).After(2000, 1));
			var wholeSecondsBetweenCalls = ((int) time2.Subtract(time1).TotalSeconds);
			wholeSecondsBetweenCalls.Should().Be(1);
		}

		[Test]
		public void ShouldAbortOnDispose()
		{
			var executed = false;
			var target = new ActionThrottle(100);

			target.Do(() => { });
			target.Do(() => { });
			target.Do(() => { executed = true; });
			target.Start();
			target.Dispose();
			Thread.Sleep(50);

			executed.Should().Be.False();
		}

		[Test]
		public void ShouldNotLeakMemory()
		{
			Assert.Ignore("Ofcourse not, imples using a queue and not a list that increases continously");
		}

		[Test]
		public void ShouldBeThreadSafe()
		{
			Assert.Ignore("Hard to test, implies using a concurrent queue or something");
		}

	}
}