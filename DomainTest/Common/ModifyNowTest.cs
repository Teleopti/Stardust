using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
	[TestFixture]
	public class ModifyNowTest
	{
		[Test]
		public void ShouldReturnFixedDate()
		{
			var date = new DateTime(2000, 1, 1, 2, 3, 4);
			var cccTimeZone = (TimeZoneInfo.Local);
			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			userTimeZone.Expect(mock => mock.TimeZone()).Return(cccTimeZone);
			var target = new Now(() => userTimeZone);
			((IModifyNow)target).SetNow(date);

			target.DateOnly().Should().Be.EqualTo(new DateOnly(date));
			target.LocalDateTime().Should().Be.EqualTo(date);
			target.UtcDateTime().Should().Be.EqualTo(TimeZoneHelper.ConvertToUtc(date, cccTimeZone));
		}

		[Test]
		public void ShouldReturnFixedTimeAsUtcIfNotLoggedOn()
		{
			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			userTimeZone.Expect(mock => mock.TimeZone()).Return(null);
			var fixedDate = new DateTime(2000, 1, 1, 2, 3, 4);
			var target = new Now(() => userTimeZone);
			((IModifyNow)target).SetNow(fixedDate);

			target.DateOnly().Should().Be.EqualTo(new DateOnly(fixedDate));
			target.LocalDateTime().Should().Be.EqualTo(fixedDate);
			target.UtcDateTime().Should().Be.EqualTo(fixedDate);
		}

		[Test]
		public void CanResetUsingRealTime()
		{
			var target = new Now(null);
			var nu = DateTime.UtcNow;
			((IModifyNow)target).SetNow(nu.AddYears(2));
			((IModifyNow)target).SetNow(null);

			target.UtcDateTime().Should().Be.IncludedIn(nu.AddMinutes(-1), nu.AddMinutes(1));
		}

		[Test]
		public void ShouldBeExplicitlySet()
		{
			var target = new Now(null);
			((IModifyNow)target).SetNow(DateTime.Now);
			target.IsExplicitlySet().Should().Be.True();
		}

		[Test]
		public void ShouldNotBeExplicitlySet()
		{
			var target = new Now(null);
			target.IsExplicitlySet().Should().Be.False();
		}
	}
}