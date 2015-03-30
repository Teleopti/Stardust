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
		private DateOnly _testDate;

		[SetUp]
		public void SetUp()
		{
			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			var timeZone = TimeZoneInfoFactory.StockholmTimeZoneInfo();
			_testDate = new DateOnly(2015, 03, 02);
			userTimeZone.Expect(c => c.TimeZone()).Return(timeZone);
			_filterHelper = new TimeFilterHelper(userTimeZone);
		}

		[Test]
		public void ShouldGetFilterOnlyWithDayoff()
		{
			var result = _filterHelper.GetFilter(_testDate, null, null, true, false);

			result.IsDayOff.Should().Be.True();
			result.IsEmptyDay.Should().Be.False();
		}

		[Test]
		public void ShouldGetFilterOnlyWithEmptyDay()
		{
			var result = _filterHelper.GetFilter(_testDate, null, null, false, true);

			result.IsDayOff.Should().Be.False();
			result.IsEmptyDay.Should().Be.True();
		}

		[Test]
		public void ShouldGetFilterWithNothing()
		{
			var result = _filterHelper.GetFilter(_testDate, null, null, false, false);

			result.Should().Be.Null();
		}

		[Test]
		public void ShouldGetFilterWithStartTimeAsUtc()
		{
			const string startTime = "8:00-10:00";
			var result = _filterHelper.GetFilter(_testDate, startTime, null, false, false);

			var utcTime = new DateTime(2015, 3, 2, 7, 0, 0, DateTimeKind.Utc);

			result.StartTimes.First().StartDateTime.Should().Be.EqualTo(utcTime);
			result.StartTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(2));
		}

		[Test]
		public void ShouldGetFilterWithEndTimeAsUtc()
		{
			const string endTime = "8:00-10:00";
			var result = _filterHelper.GetFilter(_testDate, null, endTime, false, false);

			var utcTime = new DateTime(2015, 3, 2, 7, 0, 0, DateTimeKind.Utc);

			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(utcTime);
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(2));
			result.IsDayOff.Should().Be.False();
			result.IsEmptyDay.Should().Be.False();
		}

		[Test]
		public void ShouldGetFilterWithBothTimeAsUtcAndDayoff()
		{
			const string startTime = "8:00-10:00";
			const string endTime = "16:00-18:00";
			const bool isDayOff = true;
			var result = _filterHelper.GetFilter(_testDate, startTime, endTime, isDayOff, false);

			var utcTime = new DateTime(2015, 3, 2, 7, 0, 0, DateTimeKind.Utc);

			result.StartTimes.First().StartDateTime.Should().Be.EqualTo(utcTime);
			result.StartTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(2));
			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(utcTime.AddHours(8));
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(10));
			result.IsDayOff.Should().Be.True();
			result.IsEmptyDay.Should().Be.False();
		}

		[Test]
		public void ShouldGetFilterWithBothTimeAsUtcAndEmptyDay()
		{
			const string startTime = "8:00-10:00";
			const string endTime = "16:00-18:00";
			const bool isEmptyDay = true;
			var result = _filterHelper.GetFilter(_testDate, startTime, endTime, false, isEmptyDay);

			var utcTime = new DateTime(2015, 3, 2, 7, 0, 0, DateTimeKind.Utc);

			result.StartTimes.First().StartDateTime.Should().Be.EqualTo(utcTime);
			result.StartTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(2));
			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(utcTime.AddHours(8));
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(10));
			result.IsDayOff.Should().Be.False();
			result.IsEmptyDay.Should().Be.True();
		}

		[Test]
		public void ShouldGetFilterForNightShift()
		{
			const string endTime = "06:00-08:00";
			var result = _filterHelper.GetFilter(_testDate, "", endTime, false, false);

			var utcTime = new DateTime(2015, 3, 2, 5, 0, 0, DateTimeKind.Utc);

			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(utcTime);
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(utcTime.AddHours(2));
			result.EndTimes.Last().StartDateTime.Should().Be.EqualTo(utcTime.AddDays(1));
			result.EndTimes.Last().EndDateTime.Should().Be.EqualTo(utcTime.AddDays(1).AddHours(2));
		}

		[Test]
		public void ShouldForGetFilterWithPlusEndTime()
		{
			const string startTime = "06:00-08:00";
			var result = _filterHelper.GetFilter(_testDate, startTime, "", false, false);

			var endutcTimeStart = new DateTime(2015, 3, 1, 23, 0, 0, DateTimeKind.Utc);
			var endutcTimeEnd = new DateTime(2015, 3, 3, 23, 0, 0, DateTimeKind.Utc);

			result.EndTimes.First().StartDateTime.Should().Be.EqualTo(endutcTimeStart);
			result.EndTimes.First().EndDateTime.Should().Be.EqualTo(endutcTimeEnd);
		}
	}
}
