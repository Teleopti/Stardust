using System;
using NUnit.Framework;
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
			var target = new UserInfoController(cultureStub, new FakeUserTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));
			
			var result = target.Culture();
			dynamic content = result.Data;

			Assert.That((object)content.WeekStart,Is.EqualTo(1));
			Assert.That(result,Is.Not.Null);
		}

		[Test]
		public void Culture_DateFormatIsCorrectForSwedishCulture()
		{
			var cultureStub = new SwedishCulture();
			var target = new UserInfoController(cultureStub, new FakeUserTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));

			var result = target.Culture();
			dynamic content = result.Data;

			Assert.That((object)content.DateFormatForMoment, Is.EqualTo("YYYY-MM-DD"));
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void Culture_BaseUtcOffsetInMinutesIsCorrectForSwedishCulture()
		{
			var cultureStub = new SwedishCulture();
			var target = new UserInfoController(cultureStub, new FakeUserTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")));

			var result = target.Culture();
			dynamic content = result.Data;

			Assert.That((object)content.BaseUtcOffsetInMinutes, Is.EqualTo(60));
			Assert.That(result, Is.Not.Null);
		}
	}
}