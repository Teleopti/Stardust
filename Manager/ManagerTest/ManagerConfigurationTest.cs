using System;
using NUnit.Framework;
using Stardust.Manager;

namespace ManagerTest
{
	class ManagerConfigurationTest
	{
		[Test]
		public void ShouldThrowExceptionWhenBaseAddressIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => new ManagerConfiguration(null, "Route", 1, 1, 1, 1, 1, 1));
		}

		[Test]
		public void ShouldThrowExceptionWhenRouteIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => new ManagerConfiguration("test", null, 1, 1, 1, 1, 1, 1));
		}

		[Test]
		public void ShouldThrowExceptionWhenAllowedNodeDownTimeSecondsIsZero()
		{
			Assert.Throws<ArgumentNullException>(() => new ManagerConfiguration("test", "Route", 0, 1, 1, 1, 1, 1));
		}

		[Test]
		public void ShouldThrowExceptionWhenCheckNewJobIntervalSecondsIsZero()
		{
			Assert.Throws<ArgumentNullException>(() => new ManagerConfiguration("test", "Route", 1, 0, 1, 1, 1, 1));
		}
	}
}
