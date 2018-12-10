using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Shared;


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


		[Test]
		public void ShouldNotAcceptStartTimeEqualsToEndTime()
		{
			var form = new DateTimePeriodForm
			{
				StartDate = DateOnly.Today,
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndDate = DateOnly.Today,
				EndTime = new TimeOfDay(TimeSpan.FromHours(8))
			};

			var target = new DateTimePeriodFormValidator();

			var result = target.Validate(form);

			result.Single().ErrorMessage.Should().Be(string.Format(Resources.InvalidTimeValue, Resources.Period));
		}

		[Test]
		public void ShouldNotAcceptDateOutOfSmallDateTimeRange()
		{
			var date = new DateTime(1760, 4, 30);

			var form = new DateTimePeriodForm
			{
				StartDate = new DateOnly() { Date = date },
				StartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				EndDate = new DateOnly() { Date = date },
				EndTime = new TimeOfDay(TimeSpan.FromHours(11))
			};

			var target = new DateTimePeriodFormValidator();

			var result = target.Validate(form).ToArray();

			result[0].ErrorMessage.Should().Be(string.Format(Resources.InvalidTimeValue, Resources.RequestStartDate));
			result[1].ErrorMessage.Should().Be(string.Format(Resources.InvalidTimeValue, Resources.RequestEndDate));
		}
	}
}
