using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class UserInfoControllerTest
	{
		[Test]
		public void Culture_WhenUsersCultureHasMondayAsFirstDay_ShouldSetWeekStartTo1()
		{
			var cultureStub = new SwedishCulture();
			var target = new UserInfoController(cultureStub, new FakeUserTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")), new MutableNow());
			
			var result = target.Culture();
			dynamic content = result.Data;

			Assert.That((object)content.WeekStart,Is.EqualTo(1));
			Assert.That(result,Is.Not.Null);
		}

		[Test]
		public void Culture_DateFormatIsCorrectForSwedishCulture()
		{
			var cultureStub = new SwedishCulture();
			var target = new UserInfoController(cultureStub, new FakeUserTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")), new MutableNow());

			var result = target.Culture();
			dynamic content = result.Data;

			Assert.That((object)content.DateFormatForMoment, Is.EqualTo("YYYY-MM-DD"));
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void Culture_BaseUtcOffsetInMinutesIsCorrectForSwedishCulture()
		{
			var cultureStub = new SwedishCulture();
			var target = new UserInfoController(cultureStub, new FakeUserTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")), new MutableNow());

			var result = target.Culture();
			dynamic content = result.Data;

			Assert.That((object)content.BaseUtcOffsetInMinutes, Is.EqualTo(60));
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void Culture_DaylightSavingTimeAdjustment()
		{
			var cultureStub = new SwedishCulture();
			var now = new MutableNow();
			now.Is(new DateTime(2017, 8, 16, 8, 0, 0, DateTimeKind.Utc));
			var target = new UserInfoController(cultureStub, new FakeUserTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")), now);

			var result = target.Culture();
			dynamic content = result.Data;

			Assert.That((object)content.DaylightSavingTimeAdjustment.AdjustmentOffsetInMinutes, Is.EqualTo(60));
			Assert.That((object)content.DaylightSavingTimeAdjustment.StartDateTime, Is.EqualTo(new DateTime(2017,3,26,1,0,0,DateTimeKind.Utc)));
			Assert.That((object)content.DaylightSavingTimeAdjustment.EndDateTime, Is.EqualTo(new DateTime(2017,10,29,2,0,0,DateTimeKind.Utc)));
			Assert.That(result, Is.Not.Null);
		}
	}
}