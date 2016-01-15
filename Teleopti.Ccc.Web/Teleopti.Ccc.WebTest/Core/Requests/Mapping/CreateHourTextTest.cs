using System;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class CreateHourTextTest
	{
		[Test]
		public void ShouldCreateHourTextInSwedish()
		{
			var target = new CreateHourText(new FakeUserTimeZone(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time")), new SwedishCulture());

			var result = target.CreateText(new DateTime(2013, 9, 30, 8, 0, 0, DateTimeKind.Utc));

			result.Should().Be.EqualTo("10:00");
		}

		[Test]
		public void ShouldCreateHourTextInEnglishUnitedStates()
		{
			var target = new CreateHourText(new FakeUserTimeZone(TimeZoneInfo.Utc), new FakeUserCulture(CultureInfo.GetCultureInfo("en-US")));

			var result = target.CreateText(new DateTime(2013, 9, 30, 8, 0, 0, DateTimeKind.Utc));

			result.Should().Be.EqualTo("8:00 AM");
		}
	}
}