using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.Mapping
{
	[TestFixture]
	public class ShiftTradeTimeLineHoursViewModelFactoryTest
	{
		[Test]
		public void ShouldHandleTimeLineStartNotOnTheHour()
		{
			var target = new ShiftTradeTimeLineHoursViewModelFactory(MockRepository.GenerateMock<ICreateHourText>(),
			                                                         MockRepository.GenerateMock<IUserTimeZone>());

			var period = new DateTimePeriod(new DateTime(2013, 9, 30, 7, 45, 0, DateTimeKind.Utc),
			                                new DateTime(2013, 9, 30, 17, 0, 0, DateTimeKind.Utc));
			var result = target.CreateTimeLineHours(period);

			var firstHour = result.First();
			firstHour.HourText.Should().Be.Empty();
			firstHour.LengthInMinutesToDisplay.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldHandleTimeLineEndNotOnTheHour()
		{
			var createHourText = MockRepository.GenerateMock<ICreateHourText>();

			var target = new ShiftTradeTimeLineHoursViewModelFactory(createHourText, MockRepository.GenerateMock<IUserTimeZone>());
			
			var period = new DateTimePeriod(new DateTime(2013, 9, 30, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2013, 9, 30, 17, 15, 0, DateTimeKind.Utc));
			createHourText.Stub(x => x.CreateText(period.EndDateTime)).Return("19");
			var result = target.CreateTimeLineHours(period);

			var lastHour = result.Last();
			lastHour.HourText.Should().Be.EqualTo("19");
			lastHour.LengthInMinutesToDisplay.Should().Be.EqualTo(15);
		}

		[Test]
		public void ShouldContainCorrectAmountOfTimeLineHoursWhenStartingOnTheHour()
		{
			var target = new ShiftTradeTimeLineHoursViewModelFactory(MockRepository.GenerateMock<ICreateHourText>(),
			                                                         MockRepository.GenerateMock<IUserTimeZone>());

			var period = new DateTimePeriod(new DateTime(2013, 9, 30, 8, 0, 0, DateTimeKind.Utc),
											new DateTime(2013, 9, 30, 9, 0, 0, DateTimeKind.Utc));
			
			target.CreateTimeLineHours(period).Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldContainCorrectAmountOfTimeLineHoursWhenStartingNotOnTheHour()
		{
			var target = new ShiftTradeTimeLineHoursViewModelFactory(MockRepository.GenerateMock<ICreateHourText>(),
			                                                         MockRepository.GenerateMock<IUserTimeZone>());

			var period = new DateTimePeriod(new DateTime(2013, 9, 30, 7, 45, 0, DateTimeKind.Utc),
											new DateTime(2013, 9, 30, 9, 0, 0, DateTimeKind.Utc));

			target.CreateTimeLineHours(period).Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldHaveStartAndEndTimeForHours()
		{
			var timeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var userTimeZone = MockRepository.GenerateMock<IUserTimeZone>();
			userTimeZone.Stub(x => x.TimeZone()).Return(timeZone);

			var target = new ShiftTradeTimeLineHoursViewModelFactory(MockRepository.GenerateMock<ICreateHourText>(), userTimeZone);

			var period = new DateTimePeriod(new DateTime(2013, 9, 30, 7, 45, 0, DateTimeKind.Utc),
											new DateTime(2013, 9, 30, 17, 0, 0, DateTimeKind.Utc));
			var result = target.CreateTimeLineHours(period);

			var firstHour = result.First();
			var lastHour = result.Last();

			firstHour.StartTime.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(period.StartDateTime, timeZone));
			firstHour.EndTime.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(new DateTime(2013, 9, 30, 8, 0, 0, DateTimeKind.Utc), timeZone));

			lastHour.StartTime.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(new DateTime(2013, 9, 30, 16, 0, 0, DateTimeKind.Utc), timeZone));
			lastHour.EndTime.Should().Be.EqualTo(TimeZoneHelper.ConvertFromUtc(period.EndDateTime, timeZone));
		}
	}
}
