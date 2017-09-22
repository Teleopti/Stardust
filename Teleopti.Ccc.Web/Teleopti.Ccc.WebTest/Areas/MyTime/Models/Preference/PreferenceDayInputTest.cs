using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Models.Preference
{
	[TestFixture]
	public class PreferenceDayInputTest
	{
		[Test]
		public void ShouldAcceptCorrectTimeOfDays()
		{
			var input = new PreferenceDayInput
				{
					EarliestStartTime = new TimeOfDay(TimeSpan.FromHours(8)),
					LatestStartTime = new TimeOfDay(TimeSpan.FromHours(9)),
					EarliestEndTime = new TimeOfDay(TimeSpan.FromHours(8)),
					LatestEndTime = new TimeOfDay(TimeSpan.FromHours(9)),
					ActivityEarliestStartTime = new TimeOfDay(TimeSpan.FromHours(8)),
					ActivityLatestStartTime = new TimeOfDay(TimeSpan.FromHours(9)),
					ActivityEarliestEndTime = new TimeOfDay(TimeSpan.FromHours(8)),
					ActivityLatestEndTime = new TimeOfDay(TimeSpan.FromHours(9)),
				};

			var result = input.Validate(null);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotAcceptStartTimeLaterThanEndTime()
		{
			var input = new PreferenceDayInput
			{
				EarliestStartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				LatestStartTime = new TimeOfDay(TimeSpan.FromHours(7)),
				EarliestEndTime = new TimeOfDay(TimeSpan.FromHours(8)),
				LatestEndTime = new TimeOfDay(TimeSpan.FromHours(7)),
				ActivityEarliestStartTime = new TimeOfDay(TimeSpan.FromHours(8)),
				ActivityLatestStartTime = new TimeOfDay(TimeSpan.FromHours(7)),
				ActivityEarliestEndTime = new TimeOfDay(TimeSpan.FromHours(8)),
				ActivityLatestEndTime = new TimeOfDay(TimeSpan.FromHours(7)),
			};

			var result = input.Validate(null).ToArray();

			result.Count().Should().Be(4);
			result.First().ErrorMessage.Should().Be(string.Format(Resources.InvalidTimeValue, Resources.StartTime));
		}

		[Test]
		public void ShouldAcceptCorrectTimesWithNextDay()
		{
			var input = new PreferenceDayInput
			{
				EarliestEndTime = new TimeOfDay(TimeSpan.FromHours(23)),
				LatestEndTime = new TimeOfDay(TimeSpan.FromHours(1)),
				LatestEndTimeNextDay = true
			};

			var result = input.Validate(null);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotAcceptWrongTimesWithNextDay()
		{
			var input = new PreferenceDayInput
			{
				EarliestEndTime = new TimeOfDay(TimeSpan.FromHours(23)),
				LatestEndTime = new TimeOfDay(TimeSpan.FromHours(1)),
				LatestEndTimeNextDay = false
			};

			var result = input.Validate(null).ToArray();

			result.Count().Should().Be(1);
			result.First().ErrorMessage.Should().Be(string.Format(Resources.InvalidTimeValue, Resources.EndTime));
		}

		[Test]
		public void ShouldAcceptCorrectTimeSpans()
		{
			var input = new PreferenceDayInput
			{
				MinimumWorkTime = TimeSpan.FromHours(8),
				MaximumWorkTime = TimeSpan.FromHours(9),
				ActivityMinimumTime = TimeSpan.FromHours(8),
				ActivityMaximumTime = TimeSpan.FromHours(9),
			};

			var result = input.Validate(null);

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldNotAcceptMinTimeShorterThanMaxTime()
		{
			var input = new PreferenceDayInput
			{
				MinimumWorkTime = TimeSpan.FromHours(8),
				MaximumWorkTime = TimeSpan.FromHours(7),
				ActivityMinimumTime = TimeSpan.FromHours(8),
				ActivityMaximumTime = TimeSpan.FromHours(7),
			};

			var result = input.Validate(null).ToArray();

			result.Count().Should().Be(2);
			result.First().ErrorMessage.Should().Be(string.Format(Resources.InvalidTimeValue, Resources.WorkTimeHeader));
		}

		[Test]
		public void ShouldNotAcceptEmptyForm()
		{
			var input = new PreferenceDayInput();

			var result = input.Validate(null).ToArray();

			result.Count().Should().Be(1);
			result.First().ErrorMessage.Should().Be(string.Format(Resources.EmptyRequest, Resources.ExtendedPreferences));
		}
	}
}
