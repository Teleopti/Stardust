using System;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.DistributedLock
{
	[TestFixture]
	[InfrastructureTest]
	public class DistributedLockTest : IExtendSystem
	{
		public IDistributedLockAcquirer Target;
		public FakeConfigReader ConfigReader;
		public Lock1 Lock1Proxy;
		
		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<Lock1>();
		}
		
		[Test]
		public void ShouldNotRunWhenLocked()
		{
			var successCount = 0;
			Target.TryLockForTypeOf(new Lock1(), () =>
			{
				successCount++;
				Target.TryLockForTypeOf(new Lock1(), () =>
				{
					successCount++;
				});
			});
			Target.TryLockForTypeOf(new Lock1(), () =>
			{
				successCount++;
			});

			successCount.Should().Be(2);
		}

		[Test]
		public void ShouldLockForEachType()
		{
			var successCount = 0;
			Target.TryLockForTypeOf(new Lock1(), () =>
			{
				successCount++;
				Target.TryLockForTypeOf(new Lock2(), () =>
				{
					successCount++;
				});
			});

			successCount.Should().Be(2);
		}

		[Test]
		public void ShouldLockOnProxyType()
		{
			var successCount = 0;
			Target.TryLockForTypeOf(Lock1Proxy, () =>
			{
				successCount++;
				Target.TryLockForTypeOf(Lock1Proxy, () =>
				{
					successCount++;
				});
			});
			Target.TryLockForTypeOf(Lock1Proxy, () =>
			{
				successCount++;
			});

			successCount.Should().Be(2);
		}

		[Test]
		public void ShouldReleaseLockOnException()
		{
			var wasReleased = false;
			var exceptionThrown = false;
			try
			{
				Target.TryLockForTypeOf(new Lock1(), () =>
				{
					throw new TestException();
				});
			}
			catch (TestException)
			{
				exceptionThrown = true;
			}
			Target.TryLockForTypeOf(new Lock1(), () =>
			{
				wasReleased = true;
			});

			wasReleased.Should().Be(true);
			exceptionThrown.Should().Be(true);
		}

		[Test]
		[Ignore("Ran manually just to see that the keep alive query actually works. Hard to test without 30 azure timeout.")]
		[Setting("DistributedLockKeepAliveInterval", 100)]
		public void ShouldKeepConnectionAliveInAzure()
		{
			Target.TryLockForTypeOf(new Lock1(), () =>
			{
				Thread.Sleep(5000);
			});
		}

		[Serializable]
		public class TestException : Exception
		{
		}

		public class Lock1
		{
		}

		public class Lock2
		{
		}
	}
}
