using System;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Infrastructure.DistributedLock;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.DistributedLock
{
	[TestFixture]
	[InfrastructureTest]
	public class DistributedLockTest : ISetup
	{
		public IDistributedLockAcquirer Target;
		public FakeConfigReader ConfigReader;
		public Lock1 Lock1Proxy;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<Lock1>();
		}

		[Test]
		public void ShouldNotRunInParallelWhenLocked()
		{
			var oneRunning = new ManualResetEventSlim(false);
			var twoRunning = new ManualResetEventSlim(false);
			var twoRanWhileOneWasRunning = true;

			var one = onAnotherThread(() =>
			{
				using (Target.LockForTypeOf(new Lock1()))
				{
					oneRunning.Set();
					twoRanWhileOneWasRunning = twoRunning.Wait(TimeSpan.FromMilliseconds(500));
				}
			});
			var two = onAnotherThread(() =>
			{
				oneRunning.Wait(TimeSpan.FromSeconds(1));
				using (Target.LockForTypeOf(new Lock1()))
				{
					twoRunning.Set();
				}
			});
			one.Join();
			two.Join();

			twoRanWhileOneWasRunning.Should().Be.False();
		}

		[Test]
		public void ShouldNotRunInParallelWhenLockedOnProxy()
		{
			var oneRunning = new ManualResetEventSlim(false);
			var twoRunning = new ManualResetEventSlim(false);
			var twoRanWhileOneWasRunning = true;

			var one = onAnotherThread(() =>
			{
				using (Target.LockForTypeOf(Lock1Proxy))
				{
					oneRunning.Set();
					twoRanWhileOneWasRunning = twoRunning.Wait(TimeSpan.FromMilliseconds(500));
				}
			});
			var two = onAnotherThread(() =>
			{
				oneRunning.Wait(TimeSpan.FromSeconds(1));
				using (Target.LockForTypeOf(new Lock1()))
				{
					twoRunning.Set();
				}
			});
			one.Join();
			two.Join();

			twoRanWhileOneWasRunning.Should().Be.False();
		}

		[Test]
		public void ShouldCreateLockForEachType()
		{
			var oneRunning = new ManualResetEventSlim(false);
			var twoRunning = new ManualResetEventSlim(false);
			var twoRanWhileOneWasRunning = false;
			var oneRanWhileTwoWasRunning = false;

			var one = onAnotherThread(() =>
			{
				using (Target.LockForTypeOf(new Lock1()))
				{
					oneRunning.Set();
					twoRanWhileOneWasRunning = twoRunning.Wait(TimeSpan.FromMilliseconds(500));
				}
			});
			var two = onAnotherThread(() =>
			{
				using (Target.LockForTypeOf(new Lock2()))
				{
					twoRunning.Set();
					oneRanWhileTwoWasRunning = oneRunning.Wait(TimeSpan.FromMilliseconds(500));
				}
			});
			one.Join();
			two.Join();

			twoRanWhileOneWasRunning.Should().Be.True();
			oneRanWhileTwoWasRunning.Should().Be.True();
		}

		[Test]
		[Setting("DistributedLockTimeout", 100)]
		public void ShouldNotRunOnTimeout()
		{
			var isLocking = new ManualResetEventSlim(false);
			var ran = false;

			var locking = onAnotherThread(() =>
			{
				using (Target.LockForTypeOf(new Lock1()))
				{
					isLocking.Set();
					Thread.Sleep(500);
				}
			});
			var timingout = onAnotherThread(() =>
			{
				isLocking.Wait(TimeSpan.FromMilliseconds(500));
				using (Target.LockForTypeOf(new Lock1()))
					ran = true;
			});
			locking.Join();
			timingout.Join();

			ran.Should().Be.False();
		}

		[Test]
		[Setting("DistributedLockTimeout", 100)]
		public void ShouldThrowOnTimeout()
		{
			var isLocking = new ManualResetEventSlim(false);

			var locking = onAnotherThread(() =>
			{
				using (Target.LockForTypeOf(new Lock1()))
				{
					isLocking.Set();
					Thread.Sleep(500);
				}
			});
			Exception exception = null;
			var timingout = onAnotherThread(() =>
			{
				isLocking.Wait(TimeSpan.FromMilliseconds(500));
				try
				{
					using (Target.LockForTypeOf(new Lock1()))
					{
					}
				}
				catch (Exception e)
				{
					exception = e;
				}
			});
			locking.Join();
			timingout.Join();

			exception.Should().Be.OfType<DistributedLockException>();
		}

		public class Lock1
		{
		}

		public class Lock2
		{
		}

		private static Thread onAnotherThread(Action action)
		{
			var thread = new Thread(() => action());
			thread.Start();
			return thread;
		}

	}

}
