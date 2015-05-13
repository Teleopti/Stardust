using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.TeamSchedule.Mapping
{
	[TestFixture]
	[TeamScheduleTestAttribute]
	public class TimeLineViewModelReworkedMapperTest
	{
		public ITimeLineViewModelReworkedMapper Mapper;

		[Test]
		public void ShouldMap()
		{
			var target = Mapper.Map(new List<AgentScheduleViewModelReworked>(), new DateOnly());
			target.ToList().Should().Not.Be.Null();
		}

		[Test, SetCulture("sv-SE")]
		public void ShouldMapTimeLineWithDefaultPeriodWhenEmptySchedules()
		{
			var target = Mapper.Map(new List<AgentScheduleViewModelReworked>(), new DateOnly()).ToList();
			target.First().HourText.Should().Be.EqualTo("");
			target.Second().HourText.Should().Be.EqualTo("08:00");
			target.Last().HourText.Should().Be.EqualTo("17:00");
		}
	}

	public class FakeTimeLineViewModelReworkedFactory : ITimeLineViewModelReworkedFactory
	{

		public IEnumerable<TimeLineViewModelReworked> CreateTimeLineHours(DateTimePeriod timeLinePeriod)
		{
			var hourList = new List<TimeLineViewModelReworked>();
			TimeLineViewModelReworked lastHour = null;
			var shiftStartRounded = timeLinePeriod.StartDateTime;
			var shiftEndRounded = timeLinePeriod.EndDateTime;
			var timeZone = TimeZoneInfo.Utc;

			if (timeLinePeriod.StartDateTime.Minute != 0)
			{
				var lengthInMinutes = 60 - timeLinePeriod.StartDateTime.Minute;
				hourList.Add(new TimeLineViewModelReworked
				{
					HourText = string.Empty,
					LengthInMinutesToDisplay = lengthInMinutes,
					StartTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.StartDateTime, timeZone),
					EndTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.StartDateTime.AddMinutes(lengthInMinutes), timeZone)
				});
				shiftStartRounded = timeLinePeriod.StartDateTime.AddMinutes(lengthInMinutes);
			}
			if (timeLinePeriod.EndDateTime.Minute != 0)
			{
				shiftEndRounded = timeLinePeriod.EndDateTime.AddMinutes(-timeLinePeriod.EndDateTime.Minute);
				lastHour = new TimeLineViewModelReworked
				{
					HourText = shiftEndRounded.ToShortTimeString(),
					LengthInMinutesToDisplay = timeLinePeriod.EndDateTime.Minute,
					StartTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.EndDateTime.AddMinutes(-timeLinePeriod.EndDateTime.Minute), timeZone),
					EndTime = TimeZoneHelper.ConvertFromUtc(timeLinePeriod.EndDateTime, timeZone)
				};
			}

			for (var time = shiftStartRounded; time < shiftEndRounded; time = time.AddHours(1))
			{
				hourList.Add(new TimeLineViewModelReworked
				{
					HourText = time.ToShortTimeString(),
					LengthInMinutesToDisplay = 60,
					StartTime = TimeZoneHelper.ConvertFromUtc(time, timeZone),
					EndTime = TimeZoneHelper.ConvertFromUtc(time.AddMinutes(60), timeZone)
				});
			}

			if (lastHour != null)
				hourList.Add(lastHour);

			return hourList;
		}
	}
}
