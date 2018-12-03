using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.WeekSchedule;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.WeekSchedule.Mapping
{
	[TestFixture]
	public class OvertimeAvailabilityInputMappingTest
	{
		private ILoggedOnUser loggedOnUser;
		private OvertimeAvailabilityInputMapper target;

		[SetUp]
		public void Setup()
		{
			loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			target = new OvertimeAvailabilityInputMapper(loggedOnUser);
		}
		
		[Test]
		public void ShouldMapPerson()
		{
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var result = target.Map(new OvertimeAvailabilityInput());

			result.Person.Should().Be.SameInstanceAs(person);
		}

		[Test]
		public void ShouldMapDate()
		{
			var dateOnly = DateOnly.Today;
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			var result = target.Map(new OvertimeAvailabilityInput
				{
					Date = dateOnly
				});

			result.BelongsToPeriod(new DateOnlyAsDateTimePeriod(dateOnly, TimeZoneInfo.Utc));
		}

		[Test]
		public void ShouldMapStartTime()
		{
			var timeSpan = new TimeSpan(1, 1, 1);
			var result = target.Map(new OvertimeAvailabilityInput
				{
					StartTime = new TimeOfDay(timeSpan)
				});
			result.StartTime.Should().Be.EqualTo(timeSpan);
		}

		[Test]
		public void ShouldMapEndTime()
		{
			var timeSpan = new TimeSpan(1, 1, 1);
			var result = target.Map(new OvertimeAvailabilityInput
				{
					EndTime = new TimeOfDay(timeSpan)
				});
			result.EndTime.Should().Be.EqualTo(timeSpan);
		}

		[Test]
		public void ShouldMapEndTimeNextDay()
		{
			var timeSpan = new TimeSpan(1, 1, 1);
			var result = target.Map(new OvertimeAvailabilityInput
				{
					EndTime = new TimeOfDay(timeSpan),
					EndTimeNextDay = true
				});
			result.EndTime.Should().Be.EqualTo(timeSpan.Add(new TimeSpan(1, 0, 0, 0)));
		}
	}
}