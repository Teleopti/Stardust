using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;


namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeTimeLineHoursViewModelFactoryTest
	{
		private ShiftTradeTimeLineHoursViewModelFactory _target;
		private TimeZoneInfo _timeZone;
		private IUserTimeZone _userTimeZone;

		[SetUp]
		public void Setup()
		{
			_timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			_userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			_userTimeZone.Stub(x => x.TimeZone()).Return(_timeZone);

			_target = new ShiftTradeTimeLineHoursViewModelFactory(MockRepository.GenerateMock<ICreateHourText>(), _userTimeZone);
		}

		[Test]
		public void ShouldHandleTimeLineStartNotOnTheHour()
		{
			var period = new DateTimePeriod(new DateTime(2013, 9, 30, 7, 45, 0, DateTimeKind.Utc),
			                                new DateTime(2013, 9, 30, 17, 0, 0, DateTimeKind.Utc));
			var result = _target.CreateTimeLineHours(period);

			var firstHour = result.First();
			firstHour.HourText.Should().Be.Empty();
			firstHour.LengthInMinutesToDisplay.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldHandleTimeLineEndNotOnTheHour()
		{
			var createHourText = MockRepository.GenerateMock<ICreateHourText>();

			_target = new ShiftTradeTimeLineHoursViewModelFactory(createHourText, _userTimeZone);
			
			var period = new DateTimePeriod(new DateTime(2013, 9, 30, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2013, 9, 30, 17, 15, 0, DateTimeKind.Utc));
			createHourText.Stub(x => x.CreateText(period.EndDateTime.AddMinutes(-15))).Return("19");
			var result = _target.CreateTimeLineHours(period);

			var lastHour = result.Last();
			lastHour.HourText.Should().Be.EqualTo("19");
			lastHour.LengthInMinutesToDisplay.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldContainCorrectAmountOfTimeLineHoursWhenStartingOnTheHour()
		{
			var period = new DateTimePeriod(new DateTime(2013, 9, 30, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2013, 9, 30, 9, 0, 0, DateTimeKind.Utc));
			
			_target.CreateTimeLineHours(period).Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldContainCorrectAmountOfTimeLineHoursWhenStartingNotOnTheHour()
		{
			var period = new DateTimePeriod(new DateTime(2013, 9, 30, 7, 45, 0, DateTimeKind.Utc),
											new DateTime(2013, 9, 30, 9, 0, 0, DateTimeKind.Utc));

			_target.CreateTimeLineHours(period).Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldHaveStartAndEndTimeForHours()
		{
			var period = new DateTimePeriod(new DateTime(2013, 9, 30, 7, 45, 0, DateTimeKind.Utc),
											new DateTime(2013, 9, 30, 17, 0, 0, DateTimeKind.Utc));
			var result = _target.CreateTimeLineHours(period);

			var firstHour = result.First();
			var lastHour = result.Last();

			firstHour.StartTime.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(period.StartDateTime, _timeZone));
			firstHour.EndTime.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(new DateTime(2013, 9, 30, 8, 0, 0, DateTimeKind.Utc), _timeZone));

			lastHour.StartTime.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(new DateTime(2013, 9, 30, 16, 0, 0, DateTimeKind.Utc), _timeZone));
			lastHour.EndTime.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(period.EndDateTime, _timeZone));
		}
	}
}
