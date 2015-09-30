using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Ccc.DomainTest.MessageBroker.ImplementationDetailTests
{
	[TestFixture]
	public class ActionThrottleTest
	{
		[Test]
		public void ShouldNotExecuteActionUntilStarted()
		{
			var executed = false;
			var target = new ActionThrottleUnderTest();

			target.Do(() => { executed = true; });
			target.SignalHasWaited();

			executed.Should().Be.False();
		}

		[Test]
		public void ShouldEventuallyExecuteActions()
		{
			var executed = 0;
			var target = new ActionThrottleUnderTest();

			target.Do(() => { executed++; });
			target.Do(() => { executed++; });
			target.Start();
			target.WaitUntillStarted();

			Assert.That(() => executed, Is.EqualTo(1).After(1000, 1));
			target.SignalHasWaited();
			Assert.That(() => executed, Is.EqualTo(2).After(1000, 1));
		}

		[Test]
		public void ShouldExecuteActionsOnAnotherThread()
		{
			var threadId1 = 0;
			var threadId2 = 0;
			var target = new ActionThrottleUnderTest();

			target.Do(() => { threadId1 = Thread.CurrentThread.ManagedThreadId; });
			target.Do(() => { threadId2 = Thread.CurrentThread.ManagedThreadId; });
			target.Start();
			target.WaitUntillStarted();

			Assert.That(() => threadId1, Is.Not.EqualTo(0).After(1000, 1));
			threadId1.Should().Not.Be(Thread.CurrentThread.ManagedThreadId);
			target.SignalHasWaited();
			Assert.That(() => threadId2, Is.Not.EqualTo(0).After(1000, 1));
			threadId2.Should().Be(threadId1);
		}

		[Test]
		public void ShouldAcceptThrottleArgument()
		{
			const int actionsPerSecond = 1;
			var target = new ActionThrottleUnderTest(actionsPerSecond);

			target.Do(() => { });
			target.Start();
			target.WaitUntillStarted();

			Assert.That(() => target.WaitedMilliseconds, Is.EqualTo(1000).After(1000, 1));
		}

		[Test]
		public void ShouldAbortOnDispose()
		{
			var executed = 0;
			var target = new ActionThrottleUnderTest();

			target.Do(() => { executed++; });
			target.Do(() => { executed++; });
			target.Do(() => { executed++; });
			target.Start();
			target.WaitUntillStarted();
			target.SignalHasWaited();

			Assert.That(() => executed, Is.EqualTo(2).After(1000, 1));
			target.Dispose();
			target.SignalHasWaited();
			Assert.That(() => executed, Is.EqualTo(2).After(1000, 1));
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

	public class ActionThrottleUnderTest : ActionThrottle
	{
		private readonly AutoResetEvent _signal = new AutoResetEvent(false);
		private readonly AutoResetEvent _started = new AutoResetEvent(false);

		public ActionThrottleUnderTest(int actionsPerSecond)
			: base(actionsPerSecond)
		{
		}

		public ActionThrottleUnderTest()
			: this(1000)
		{
		}

		public int WaitedMilliseconds { get; set; }

		protected override void Started()
		{
			_started.Set();
		}

		public void WaitUntillStarted()
		{
			_started.WaitOne();
		}

		protected override Task WaitForNext(int waitMilliseconds)
		{
			WaitedMilliseconds = waitMilliseconds;
			_signal.WaitOne();
			return Task.FromResult(false);
		}

		public void SignalHasWaited()
		{
			_signal.Set();
		}

		public override void Dispose()
		{
			base.Dispose();
			_signal.Dispose();
		}
	}

}