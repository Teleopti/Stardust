using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	[TestWithStaticDependenciesDONOTUSE]
	public class TimeZoneGuardTest
	{
		private TimeZoneGuard _target;

		[SetUp]
		public void Setup()
		{
			_target = (TimeZoneGuard) TimeZoneGuard.Instance;
		}

		[TearDown]
		public void Teardown()
		{
			TimeZoneGuard.Instance.Set(null);
		}

		[Test]
		public void ShoudHaveDefaultTimeZone()
		{
			var target = TimeZoneGuard.Instance;
			Assert.AreEqual(TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone, target.CurrentTimeZone());
		}

		[Test]
		public void ShouldSetTimeZone()
		{
			var timeZone = TimeZoneInfo.Utc;
			_target.Set(timeZone);

			Assert.That(_target.CurrentTimeZone(), Is.EqualTo(timeZone));
		}
	}
}
