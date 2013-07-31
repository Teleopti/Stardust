using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[TestFixture]
	public class TimeZoneGuardTest
	{
		private ITimeZoneGuard _target;

		[SetUp]
		public void Setup()
		{
			_target = TimeZoneGuard.Instance;
		}

		[Test]
		public void ShouldSetTimeZone()
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
			_target.TimeZone = timeZone;

			Assert.That(_target.TimeZone, Is.EqualTo(timeZone));
		}

		[Test]
		public void ShoudHaveDefaultTimeZone()
		{
			Assert.AreEqual(TeleoptiPrincipal.Current.Regional.TimeZone, _target.TimeZone);
		}
	}
}
