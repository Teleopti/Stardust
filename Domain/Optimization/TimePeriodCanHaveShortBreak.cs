using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Contatins a merthod that decides whether the TimePeriodWithSegment has short break
    /// </summary>
    public class TimePeriodCanHaveShortBreak : ITimePeriodCanHaveShortBreak
    {
        public bool CanHaveShortBreak(ISkillExtractor skillExtractor, TimePeriodWithSegment timePeriodWithSegment)
        {
            int skillResolution = MinimumSkillResolution(skillExtractor);

            if(!skillResolutionFitsToTimePeriodsStartAndEndTime(skillResolution, timePeriodWithSegment))
                return true;

            if (!skillResolutionFitsToSegmentPeriodResolution(timePeriodWithSegment, skillResolution))
                return true;

            return false;
        }

        public bool CanHaveShortBreak(ISkillExtractor skillExtractor, IEnumerable<TimeSpan> timeSpans)
        {
            int skillResolution = MinimumSkillResolution(skillExtractor);

            foreach (TimeSpan timeSpan in timeSpans)
            {
                if (!skillResolutionFitsToSegmentPeriodResolution(timeSpan, skillResolution))
                    return true;
            }
            return false;
        }

        private static int MinimumSkillResolution(ISkillExtractor skillExtractor)
        {
            IEnumerable<ISkill> skills = skillExtractor.ExtractSkills();
            int currentSkillResolution = int.MaxValue;
            foreach (ISkill skill in skills)
                currentSkillResolution = Math.Min(currentSkillResolution, skill.DefaultResolution);
            return currentSkillResolution;
        }

        private static bool skillResolutionFitsToTimePeriodsStartAndEndTime(int skillResolution, TimePeriodWithSegment timePeriodWithSegment)
        {

            TimePeriod timePeriod = timePeriodWithSegment.Period;
            int startTimeMinutesOfDay = (int)timePeriod.StartTime.TotalMinutes;
            int endTimeMinutesOfDay = (int)timePeriod.EndTime.TotalMinutes;

            if(startTimeMinutesOfDay % skillResolution != 0)
                return false;

            if (endTimeMinutesOfDay % skillResolution != 0)
                return false;

            return true;
        }

        private static bool skillResolutionFitsToSegmentPeriodResolution(TimePeriodWithSegment timePeriodWithSegment, int skillResolution)
        {
            return skillResolutionFitsToSegmentPeriodResolution(timePeriodWithSegment.Segment, skillResolution);
        }

        private static bool skillResolutionFitsToSegmentPeriodResolution(TimeSpan segment, int skillResolution)
        {
            int segmentMinutes = (int)segment.TotalMinutes;

            return (segmentMinutes % skillResolution == 0);
        }

    }
}
