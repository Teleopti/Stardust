using System;
using NUnit.Framework;
using Stardust.Manager;

namespace ManagerTest
{
	class ManagerConfigurationTest
	{
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenBaseAddressIsNull()
		{
			new ManagerConfiguration(null, "Route", 1, 1, 1, 1, 1, 1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenRouteIsNull()
		{
			new ManagerConfiguration("test", null, 1, 1, 1, 1, 1, 1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenAllowedNodeDownTimeSecondsIsZero()
		{
			new ManagerConfiguration("test", "Route", 0, 1, 1, 1, 1, 1);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ShouldThrowExceptionWhenCheckNewJobIntervalSecondsIsZero()
		{
			new ManagerConfiguration("test", "Route", 1, 0, 1, 1, 1, 1);
		}
	}
}
