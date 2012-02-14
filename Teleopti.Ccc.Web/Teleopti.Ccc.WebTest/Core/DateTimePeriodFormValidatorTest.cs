using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core
{
	public class DateTimePeriodFormValidatorTest
	{
		[Test]
		public void ShouldAcceptCorrectDateTimePeriod()
		{
			var form = new DateTimePeriodForm
			           	{
			           		StartDate = DateOnly.Today,
			           		StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
			           		EndDate = DateOnly.Today,
			           		EndTime = new TimeOfDay(TimeSpan.FromHours(9))
			           	};

			var target = new DateTimePeriodFormValidator();

			var result = target.Validate(form);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotAcceptStartTimeLaterThenEndTime()
		{
			var form = new DateTimePeriodForm
			           	{
			           		StartDate = DateOnly.Today,
			           		StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
			           		EndDate = DateOnly.Today,
			           		EndTime = new TimeOfDay(TimeSpan.FromHours(7))
			           	};

			var target = new DateTimePeriodFormValidator();

			var result = target.Validate(form);

			result.Single().ErrorMessage.Should().Be(string.Format(Resources.InvalidTimeValue, Resources.Period));
		}
	}
}
