using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class ModifyNowTest
	{
		private INow target;
		private DateTime dateSet;

		[SetUp]
		public void Setup()
		{
			dateSet = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			target = new Now();
			((IModifyNow)target).SetNow(dateSet);
		}

		[Test]
		public void ShouldReturnFixedDate()
		{
			target.Date().Should().Be.EqualTo(new DateOnly(dateSet));
			target.LocalTime().Should().Be.EqualTo(dateSet.ToLocalTime());
			target.UtcTime().Should().Be.EqualTo(dateSet);
		}

		[Test]
		public void MustSetUsingUtc()
		{
			Assert.Throws<ArgumentException>(() =>
			     ((IModifyNow) target).SetNow(new DateTime(2000, 1, 1)));
		}

		[Test]
		public void CanResetUsingRealTime()
		{
			var nu = DateTime.UtcNow;
			((IModifyNow) target).SetNow(null);
			target.UtcTime().Should().Be.IncludedIn(nu.AddMinutes(-1), nu.AddMinutes(1));
		}
	}
}