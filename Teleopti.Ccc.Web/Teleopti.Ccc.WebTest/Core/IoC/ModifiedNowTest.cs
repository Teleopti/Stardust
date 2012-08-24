using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.IoC
{
	[TestFixture]
	public class ModifiedNowTest
	{
		[Test]
		public void ShouldReturnUtcNow()
		{
			var utcNow = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);
			var target = new ModifiedNow(utcNow);
			target.UtcTime().Should().Be.EqualTo(utcNow);
		}

		[Test]
		public void ShouldReturnLocalNow()
		{
			var utcNow = new DateTime(2000, 3, 1, 2, 1, 1, DateTimeKind.Utc);
			var target = new ModifiedNow(utcNow);
			target.LocalTime().Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(utcNow, new CccTimeZoneInfo(TimeZoneInfo.Local)));
		}

		[Test]
		public void ShouldReturnDateOnly()
		{
			var utcNow = new DateTime(2001, 3, 1, 2, 1, 1, DateTimeKind.Utc);			
			var target = new ModifiedNow(utcNow);
			target.Date().Should().Be.EqualTo(new DateOnly(TimeZoneHelper.ConvertFromUtc(utcNow, new CccTimeZoneInfo(TimeZoneInfo.Local))));
		}

		[Test]
		public void ShouldOnlyAcceptUtcTime()
		{
			Assert.Throws<ArgumentException>(() => new ModifiedNow(new DateTime(2000, 1, 1)));
		}
	}
}