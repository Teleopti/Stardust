using System;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.WeekSchedule.Mapping
{
	[TestFixture]
	public class OvertimeAvailabilityViewModelMappingTest
	{
		private OvertimeAvailabilityViewModelMapper Mapper;

		[SetUp]
		public void Setup()
		{
			Mapper = new OvertimeAvailabilityViewModelMapper(new ThisCulture(CultureInfo.GetCultureInfo(1053)));
		}

		[Test]
		public void ShouldMapHasOvertimeAvailability()
		{
			var person = new Person();
			var result =
				Mapper.Map(new OvertimeAvailability(person, DateOnly.Today,
				                                                                                          new TimeSpan(5, 0, 0),
				                                                                                          new TimeSpan(13, 0, 0)));

			result.HasOvertimeAvailability.Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldMapStartTime()
		{
			var person = new Person();
			var result =
				Mapper.Map(new OvertimeAvailability(person, DateOnly.Today,
				                                                                                          new TimeSpan(5, 0, 0),
				                                                                                          new TimeSpan(13, 0, 0)));

			result.StartTime.Should().Be.EqualTo("05:00");
		}

		[Test]
		public void ShouldMapEndTime()
		{
			var person = new Person();
			var result =
				Mapper.Map(new OvertimeAvailability(person, DateOnly.Today,
				                                                                                          new TimeSpan(5, 0, 0),
				                                                                                          new TimeSpan(13, 0, 0)));

			result.EndTime.Should().Be.EqualTo("13:00");
		}

		[Test]
		public void ShouldMapEndTimeNextDay()
		{
			var person = new Person();
			var result =
				Mapper.Map(new OvertimeAvailability(person, DateOnly.Today,
				                                                                                          new TimeSpan(5, 0, 0),
				                                                                                          new TimeSpan(25, 0, 0)));

			result.EndTimeNextDay.Should().Be.EqualTo(true);
		}
	}
}