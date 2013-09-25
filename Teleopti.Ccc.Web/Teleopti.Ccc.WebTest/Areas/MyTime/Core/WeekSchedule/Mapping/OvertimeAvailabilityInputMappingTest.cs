using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.WeekSchedule.Mapping
{
	[TestFixture]
	public class OvertimeAvailabilityInputMappingTest
	{
		private ILoggedOnUser loggedOnUser;
		[SetUp]
		public void Setup()
		{
			loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			Mapper.Reset();
			Mapper.Initialize(c => c.AddProfile(new OvertimeAvailabilityInputMappingProfile(loggedOnUser)));
		}

		[Test]
		public void ShouldConfigureCorrectly() { Mapper.AssertConfigurationIsValid(); }

		[Test]
		public void ShouldMapPerson()
		{
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			var result = Mapper.Map<OvertimeAvailabilityInput, IOvertimeAvailability>(new OvertimeAvailabilityInput());

			result.Person.Should().Be.SameInstanceAs(person);
		}

		[Test]
		public void ShouldMapDate()
		{
			var dateOnly = DateOnly.Today;
			var person = new Person();
			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			var result = Mapper.Map<OvertimeAvailabilityInput, IOvertimeAvailability>(new OvertimeAvailabilityInput
				{
					Date = dateOnly
				});

			result.BelongsToPeriod(new DateOnlyAsDateTimePeriod(dateOnly, TimeZoneInfo.Utc));
		}

		[Test]
		public void ShouldMapStartTime()
		{
			var timeSpan = new TimeSpan(1, 1, 1);
			var result = Mapper.Map<OvertimeAvailabilityInput, IOvertimeAvailability>(new OvertimeAvailabilityInput
				{
					StartTime = new TimeOfDay(timeSpan)
				});
			result.StartTime.Should().Be.EqualTo(timeSpan);
		}

		[Test]
		public void ShouldMapEndTime()
		{
			var timeSpan = new TimeSpan(1, 1, 1);
			var result = Mapper.Map<OvertimeAvailabilityInput, IOvertimeAvailability>(new OvertimeAvailabilityInput
				{
					EndTime = new TimeOfDay(timeSpan)
				});
			result.EndTime.Should().Be.EqualTo(timeSpan);
		}

		[Test]
		public void ShouldMapEndTimeNextDay()
		{
			var timeSpan = new TimeSpan(1, 1, 1);
			var result = Mapper.Map<OvertimeAvailabilityInput, IOvertimeAvailability>(new OvertimeAvailabilityInput
				{
					EndTime = new TimeOfDay(timeSpan),
					EndTimeNextDay = true
				});
			result.EndTime.Should().Be.EqualTo(timeSpan.Add(new TimeSpan(1, 0, 0, 0)));
		}
	}
}