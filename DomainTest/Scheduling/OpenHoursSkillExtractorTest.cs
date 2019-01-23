using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
	[DomainTest]
	public class OpenHoursSkillExtractorTest
	{
		public OpenHoursSkillExtractor Target;

		[Test]
		public void ShouldRestrictOnStartAndEndTime()
		{
			var date = new DateOnly(2018, 10, 1);
			var activity = new Activity { RequiresSkill = true }.WithId();
			var scenario = new Scenario();
			var skill = new Skill().For(activity).WithId().InTimeZone(TimeZoneInfo.Utc).IsOpenBetween(8, 16);
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(skill);
			var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 1);

			var result = Target.Extract(new []{agent}, new[] { skillDay }, date.ToDateOnlyPeriod());

			result.OpenHoursDictionary[date].StartTimeLimitation.StartTime.Should().Be.EqualTo(TimeSpan.FromHours(8));
			result.OpenHoursDictionary[date].EndTimeLimitation.EndTime.Should().Be.EqualTo(TimeSpan.FromHours(16));
			result.ForCurrentDate(date).TotalHours.Should().Be.EqualTo(8);
		}
	}
}
