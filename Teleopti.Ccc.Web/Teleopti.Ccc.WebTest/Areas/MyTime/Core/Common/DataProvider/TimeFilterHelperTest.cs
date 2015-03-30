using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Common.DataProvider
{
	[TestFixture]
	public class TimeFilterHelperTest
	{
		private TimeFilterHelper _filterHelper;

		[SetUp]
		public void SetUp()
		{
			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			var timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			userTimeZone.Expect(c => c.TimeZone()).Return(timeZone);
			_filterHelper = new TimeFilterHelper(userTimeZone);
		}

		[Test]
		public void ShouldGetFilterOnlyWithDayoff()
		{
			var result = _filterHelper.GetFilter(DateOnly.Today, null, null, true, false);

			result.IsDayOff.Should().Be.True();
			result.IsEmptyDay.Should().Be.False();
		}

		[Test]
		public void ShouldGetFilterOnlyWithEmptyDay()
		{
			var result = _filterHelper.GetFilter(DateOnly.Today, null, null, false, true);

			result.IsDayOff.Should().Be.False();
			result.IsEmptyDay.Should().Be.True();
		}

		[Test]
		public void ShouldGetFilterWithNothing()
		{
			var result = _filterHelper.GetFilter(DateOnly.Today, null, null, false, false);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldGetFilterWithStartTimeAsUtc()
		{
			const string startTime = "8:00-10:00";
			var result = _filterHelper.GetFilter(DateOnly.Today, startTime, null, false, false);

			var start = DateTime.Today.Add(TimeSpan.FromHours(7));
			var end = DateTime.Today.Add(TimeSpan.FromHours(9));

			result.StartTimes.First().StartDateTime.Should().Be.EqualTo(start);
			result.StartTimes.First().EndDateTime.Should().Be.EqualTo(end);
		}

		[Test]
		public void ShouldGetFilterWithEndTimeAsUtc()
		{
			const string endTime = "8:00-10:00";
			var result = _filterHelper.GetFilter(DateOnly.Today, null, endTime, false, false);

			var start = DateTime.Today.Add(TimeSpan.FromHours(7));
			var end = DateTime.Today.Add(TimeSpan.FromHours(9));

			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(start);
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(end);
			result.IsDayOff.Should().Be.False();
			result.IsEmptyDay.Should().Be.False();
		}

		[Test]
		public void ShouldGetFilterWithBothTimeAsUtcAndDayoff()
		{
			const string startTime = "8:00-10:00";
			const string endTime = "16:00-18:00";
			const bool isDayOff = true;
			var result = _filterHelper.GetFilter(DateOnly.Today, startTime, endTime, isDayOff, false);

			var startTimeStart = DateTime.Today.Add(TimeSpan.FromHours(7));
			var startTimesEnd = DateTime.Today.Add(TimeSpan.FromHours(9));
			var endTimeStart = DateTime.Today.Add(TimeSpan.FromHours(15));
			var endTimesEnd = DateTime.Today.Add(TimeSpan.FromHours(17));

			result.StartTimes.First().StartDateTime.Should().Be.EqualTo(startTimeStart);
			result.StartTimes.First().EndDateTime.Should().Be.EqualTo(startTimesEnd);
			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(endTimeStart);
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(endTimesEnd);
			result.IsDayOff.Should().Be.True();
			result.IsEmptyDay.Should().Be.False();
		}

		[Test]
		public void ShouldGetFilterWithBothTimeAsUtcAndEmptyDay()
		{
			const string startTime = "8:00-10:00";
			const string endTime = "16:00-18:00";
			const bool isEmptyDay = true;
			var result = _filterHelper.GetFilter(DateOnly.Today, startTime, endTime, false, isEmptyDay);

			var startTimeStart = DateTime.Today.Add(TimeSpan.FromHours(7));
			var startTimesEnd = DateTime.Today.Add(TimeSpan.FromHours(9));
			var endTimeStart = DateTime.Today.Add(TimeSpan.FromHours(15));
			var endTimesEnd = DateTime.Today.Add(TimeSpan.FromHours(17));

			result.StartTimes.First().StartDateTime.Should().Be.EqualTo(startTimeStart);
			result.StartTimes.First().EndDateTime.Should().Be.EqualTo(startTimesEnd);
			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(endTimeStart);
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(endTimesEnd);
			result.IsDayOff.Should().Be.False();
			result.IsEmptyDay.Should().Be.True();
		}

		[Test]
		public void ShouldGetFilterForNightShift()
		{
			const string endTime = "06:00-08:00";
			var result = _filterHelper.GetFilter(DateOnly.Today, "", endTime, false, false);

			var endTimeStart = DateTime.Today.Add(TimeSpan.FromHours(5));
			var endTimesEnd = DateTime.Today.Add(TimeSpan.FromHours(7));
			var endTimePlusStart = DateTime.Today.Add(TimeSpan.FromDays(1).Add(TimeSpan.FromHours(5)));
			var endTimesPlusEnd = DateTime.Today.Add(TimeSpan.FromDays(1).Add(TimeSpan.FromHours(7)));

			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(endTimeStart);
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(endTimesEnd);
			result.EndTimes.Last().StartDateTime.Should().Be.EqualTo(endTimePlusStart);
			result.EndTimes.Last().EndDateTime.Should().Be.EqualTo(endTimesPlusEnd);
		}

		[Test]
		public void ShouldForGetFilterWithPlusEndTime()
		{
			const string startTime = "06:00-08:00";
			var result = _filterHelper.GetFilter(DateOnly.Today, startTime, "", false, false);

			var endTimeStart = DateTime.Today.Add(TimeSpan.FromHours(-1));
			var endTimesEnd = DateTime.Today.Add(TimeSpan.FromDays(1).Add(TimeSpan.FromHours(23)));

			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(endTimeStart);
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(endTimesEnd);
		}
	}
}
