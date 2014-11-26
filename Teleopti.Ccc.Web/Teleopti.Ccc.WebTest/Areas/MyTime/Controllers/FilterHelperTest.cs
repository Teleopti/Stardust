using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	public class FilterHelperTest
	{
		private FilterHelper _filterHelper;

		[SetUp]
		public void SetUp()
		{
			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			var timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			userTimeZone.Expect(c => c.TimeZone()).Return(timeZone);
			_filterHelper = new FilterHelper(userTimeZone);
		}

		[Test]
		public void ShouldGetFilterOnlyWithDayoff()
		{
			var result = _filterHelper.GetFilter(DateOnly.Today, null, null, true);

			result.IsDayOff.Should().Be.EqualTo(true);
		}		
		
		[Test]
		public void ShouldGetFilterWithNothing()
		{
			var result = _filterHelper.GetFilter(DateOnly.Today, null, null, false);

			result.Should().Be.Null();
		}		
		
		[Test]
		public void ShouldGetFilterWithStartTimeAsUtc()
		{
			string startTime = "8:00-10:00";
			var result = _filterHelper.GetFilter(DateOnly.Today, startTime, null, false);

			var start = DateTime.Today.Add(TimeSpan.FromHours(7));
			var end = DateTime.Today.Add(TimeSpan.FromHours(9));

			result.StartTimes.First().StartDateTime.Should().Be.EqualTo(start);
			result.StartTimes.First().EndDateTime.Should().Be.EqualTo(end);
		}		
		
		[Test]
		public void ShouldGetFilterWithEndTimeAsUtc()
		{
			string endTime = "8:00-10:00";
			var result = _filterHelper.GetFilter(DateOnly.Today, null, endTime, false);

			var start = DateTime.Today.Add(TimeSpan.FromHours(7));
			var end = DateTime.Today.Add(TimeSpan.FromHours(9));

			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(start);
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(end);
			result.IsDayOff.Should().Be.False();
		}		
		
		[Test]
		public void ShouldGetFilterWithBothTimeAsUtcAndDayoff()
		{
			var startTime = "8:00-10:00";
			var endTime = "16:00-18:00";
			var isDayOff = true;
			var result = _filterHelper.GetFilter(DateOnly.Today, startTime, endTime, isDayOff);

			var startTimeStart = DateTime.Today.Add(TimeSpan.FromHours(7));
			var startTimesEnd = DateTime.Today.Add(TimeSpan.FromHours(9));
			var endTimeStart = DateTime.Today.Add(TimeSpan.FromHours(15));
			var endTimesEnd = DateTime.Today.Add(TimeSpan.FromHours(17));

			result.StartTimes.First().StartDateTime.Should().Be.EqualTo(startTimeStart);
			result.StartTimes.First().EndDateTime.Should().Be.EqualTo(startTimesEnd);
			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(endTimeStart);
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(endTimesEnd);
			result.IsDayOff.Should().Be.True();
		}		
		
		[Test]
		public void ShouldGetFilterForNightShift()
		{
			var endTime = "06:00-08:00";
			var result = _filterHelper.GetFilter(DateOnly.Today, "", endTime, false);

			var endTimeStart = DateTime.Today.Add(TimeSpan.FromHours(5));
			var endTimesEnd = DateTime.Today.Add(TimeSpan.FromHours(7));			
			var endTimePlusStart = DateTime.Today.Add(TimeSpan.FromDays(1).Add(TimeSpan.FromHours(5)));
			var endTimesPlusEnd = DateTime.Today.Add(TimeSpan.FromDays(1).Add(TimeSpan.FromHours(7)));

			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(endTimeStart);
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(endTimesEnd);
			result.EndTimes.Last().StartDateTime.Should().Be.EqualTo(endTimePlusStart);
			result.EndTimes.Last().EndDateTime.Should().Be.EqualTo(endTimesPlusEnd);
		}
	}
}
