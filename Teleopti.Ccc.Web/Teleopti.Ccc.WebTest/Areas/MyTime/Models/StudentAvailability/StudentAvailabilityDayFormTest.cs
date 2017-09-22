using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Models.StudentAvailability
{
	[TestFixture]
	public class StudentAvailabilityDayFormTest
	{
		[Test]
		public void ShouldAcceptCorrectTimeOfDays()
		{
			var input = new StudentAvailabilityDayInput
				{
					StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
					EndTime = new TimeOfDay(TimeSpan.FromHours(9))
				};

			var result = input.Validate(null);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotAcceptStartTimeLaterThanEndTime()
		{
			var input = new StudentAvailabilityDayInput
				{
					StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
					EndTime = new TimeOfDay(TimeSpan.FromHours(7))
				};

			var result = input.Validate(null).ToArray();

			result.Count().Should().Be(1);
			result.First().ErrorMessage.Should().Be(string.Format(Resources.InvalidTimeValue, Resources.EndTime));
		}

		[Test]
		public void ShouldAcceptCorrectTimesWithNextDay()
		{
			var input = new StudentAvailabilityDayInput
				{
					StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
					EndTime = new TimeOfDay(TimeSpan.FromHours(7)),
					NextDay = true
				};

			var result = input.Validate(null);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotAcceptWrongTimesWithNextDay()
		{
			var input = new StudentAvailabilityDayInput
				{
					StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
					EndTime = new TimeOfDay(TimeSpan.FromHours(7)),
					NextDay = false
				};

			var result = input.Validate(null).ToArray();

			result.Count().Should().Be(1);
			result.First().ErrorMessage.Should().Be(string.Format(Resources.InvalidTimeValue, Resources.EndTime));
		}
	}
}