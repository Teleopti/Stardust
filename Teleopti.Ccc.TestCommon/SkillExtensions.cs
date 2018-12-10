using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.TestCommon
{
	public static class SkillExtensions
	{
		public static ISkill IsOpen(this ISkill skill)
		{
			WorkloadFactory.CreateWorkloadWithFullOpenHours(skill);
			return skill;
		}

		public static ISkill IsOpenBetween(this ISkill skill, int startHour, int endHour)
		{
			skill.IsOpen(new TimePeriod(startHour, endHour));
			return skill;
		}

		public static ISkill IsOpen(this ISkill skill, params TimePeriod[] periods)
		{
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, periods);
			return skill;
		}

		public static ISkill IsOpen(this ISkill skill, IDictionary<DayOfWeek, TimePeriod> days)
		{
			WorkloadFactory.CreateWorkloadWithOpenHoursOnDays(skill, days);
			return skill;
		}

		public static ISkill IsOpenDuringWeekends(this ISkill skill)
		{
			WorkloadFactory.CreateWorkloadWithFullOpenHoursDuringWeekdays(skill);
			return skill;
		}

		public static ISkill IsClosed(this ISkill skill)
		{
			WorkloadFactory.CreateWorkloadThatIsClosed(skill);
			return skill;
		}

		public static ISkill IsClosedDuringWeekends(this ISkill skill)
		{
			WorkloadFactory.CreateWorkloadWithFullOpenHoursDuringWeekdaysAndClosedOnWeekends(skill);
			return skill;
		}

		public static ISkill CascadingIndex(this ISkill skill, int index)
		{
			skill.SetCascadingIndex(index);
			return skill;
		}

		public static ISkill For(this ISkill skill, IActivity activity)
		{
			skill.Activity = activity;
			return skill;
		}

		public static ISkill InTimeZone(this ISkill skill, TimeZoneInfo timeZoneInfo)
		{
			skill.TimeZone = timeZoneInfo;
			return skill;
		}

		public static ISkill DefaultResolution(this ISkill skill, int minutes)
		{
			skill.DefaultResolution = minutes;
			return skill;
		}
	}
}