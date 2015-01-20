using System;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.DistributedLock;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.DistributedLock
{
	[TestFixture]
	[InfrastructureIoCTest]
	public class DistributedLockTest
	{
		public IDistributedLockAcquirer Target;
		public FakeConfigReader ConfigReader;

		[Test]
		public void ShouldNotRunInParallelWhenLocked()
		{
			var oneRunning = new ManualResetEvent(false);
			var twoRunning = new ManualResetEvent(false);
			var twoRanWhileOneWasRunning = true;

			var one = onAnotherThread(() =>
			{
				using (Target.LockForTypeOf(new Lock1()))
				{
					oneRunning.Set();
					twoRanWhileOneWasRunning = twoRunning.WaitOne(TimeSpan.FromSeconds(1));
				}
			});
			var two = onAnotherThread(() =>
			{
				oneRunning.WaitOne(TimeSpan.FromSeconds(1));
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
			var oneRunning = new ManualResetEvent(false);
			var twoRunning = new ManualResetEvent(false);
			var twoRanWhileOneWasRunning = false;
			var oneRanWhileTwoWasRunning = false;

			var one = onAnotherThread(() =>
			{
				using (Target.LockForTypeOf(new Lock1()))
				{
					oneRunning.Set();
					twoRanWhileOneWasRunning = twoRunning.WaitOne(TimeSpan.FromSeconds(1));
				}
			});
			var two = onAnotherThread(() =>
			{
				using (Target.LockForTypeOf(new Lock2()))
				{
					twoRunning.Set();
					oneRanWhileTwoWasRunning = oneRunning.WaitOne(TimeSpan.FromSeconds(1));
				}
			});
			one.Join();
			two.Join();

			twoRanWhileOneWasRunning.Should().Be.True();
			oneRanWhileTwoWasRunning.Should().Be.True();
		}

		[Test]
		public void ShouldNotRunOnTimeout()
		{
			ConfigReader.AppSettings.Add("DistributedLockTimeout", ((int)TimeSpan.FromMilliseconds(100).TotalMilliseconds).ToString());
			var isLocking = new ManualResetEvent(false);
			var timedout = new ManualResetEvent(false);
			var ran = false;

			var locking = onAnotherThread(() =>
			{
				using (Target.LockForTypeOf(new Lock1()))
				{
					isLocking.Set();
					timedout.WaitOne(TimeSpan.FromSeconds(1));
				}
			});
			var timingout = onAnotherThread(() =>
			{
				isLocking.WaitOne(TimeSpan.FromSeconds(1));
				using (Target.LockForTypeOf(new Lock1()))
					ran = true;
				timedout.Set();
			});
			locking.Join();
			timingout.Join();

			ran.Should().Be.False();
		}

		private class Lock1
		{
		}

		private class Lock2
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
