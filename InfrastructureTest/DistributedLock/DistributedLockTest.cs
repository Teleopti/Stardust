using System;
using System.Configuration;
using System.Threading;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.DistributedLock;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.DistributedLock
{
	[TestFixture]
	[IoCTest]
	public class DistributedLockTest : IRegisterInContainer
	{
		public IDistributedLockAcquirer Target;
		public FakeConfigReader ConfigReader;

		public void RegisterInContainer(ContainerBuilder builder, IIocConfiguration configuration)
		{
			builder.RegisterInstance(
				new FakeConfigReader
				{
					ConnectionStrings = new ConnectionStringSettingsCollection
					{
						new ConnectionStringSettings("RtaApplication", ConnectionStringHelper.ConnectionStringUsedInTests)
					}
				})
				.As<IConfigReader>().AsSelf().SingleInstance();
		}

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
					twoRanWhileOneWasRunning = twoRunning.WaitOne(TimeSpan.FromMilliseconds(500));
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
					twoRanWhileOneWasRunning = twoRunning.WaitOne(TimeSpan.FromMilliseconds(500));
				}
			});
			var two = onAnotherThread(() =>
			{
				using (Target.LockForTypeOf(new Lock2()))
				{
					twoRunning.Set();
					oneRanWhileTwoWasRunning = oneRunning.WaitOne(TimeSpan.FromMilliseconds(500));
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
				isLocking.WaitOne(TimeSpan.FromMilliseconds(500));
				using (Target.LockForTypeOf(new Lock1()))
					ran = true;
			});
			locking.Join();
			timingout.Join();

			ran.Should().Be.False();
		}

		[Test]
		public void ShouldThrowOnTimeout()
		{
			ConfigReader.AppSettings.Add("DistributedLockTimeout", ((int)TimeSpan.FromMilliseconds(100).TotalMilliseconds).ToString());
			var isLocking = new ManualResetEvent(false);

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
				isLocking.WaitOne(TimeSpan.FromMilliseconds(500));
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
