using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;


namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class DateTimePeriodFormMappingTest
	{
		[Test]
		public void ShouldMapToUtcPeriod()
		{
			var timeZone = TimeZoneInfo.CreateCustomTimeZone("tzid", TimeSpan.FromHours(11), "", "");
			var periodForm = new DateTimePeriodForm
			             	{
			             		StartDate = DateOnly.Today,
			             		StartTime = new TimeOfDay(TimeSpan.FromHours(12)),
			             		EndDate = DateOnly.Today,
			             		EndTime = new TimeOfDay(TimeSpan.FromHours(13))
			             	};

			var result = periodForm.Map(new FakeUserTimeZone(timeZone));

			var expected = new DateTimePeriod(
				TimeZoneHelper.ConvertToUtc(periodForm.StartDate.Date.Add(periodForm.StartTime.Time), timeZone),
				TimeZoneHelper.ConvertToUtc(periodForm.EndDate.Date.Add(periodForm.EndTime.Time), timeZone)
				);
			result.Should().Be(expected);
		}
	}
}
