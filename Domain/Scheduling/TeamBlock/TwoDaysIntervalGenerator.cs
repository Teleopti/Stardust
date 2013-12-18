using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    
    public interface ITwoDaysIntervalGenerator
    {
        Dictionary<DateOnly, Dictionary<TimeSpan, ISkillIntervalData>> GenerateTwoDaysInterval(
            IEnumerable<KeyValuePair<DateOnly, IList<ISkillIntervalData>>> dayIntervalData, int resolution);
    }

    public class TwoDaysIntervalGenerator : ITwoDaysIntervalGenerator 
    {
        public  Dictionary<DateOnly, Dictionary<TimeSpan, ISkillIntervalData>> GenerateTwoDaysInterval(IEnumerable<KeyValuePair<DateOnly, IList<ISkillIntervalData>>> dayIntervalData, int resolution)
        {
            var twoDayIntervalsForAllDays = new Dictionary<DateOnly, Dictionary<TimeSpan, ISkillIntervalData>>();

            foreach (var dayInterval in dayIntervalData)
            {
                var firstDayBaseDate = new DateOnly(dayInterval.Value.Select(x => x.Period.StartDateTime.Date).Min());
                var intervalToSkillInterval = new Dictionary<TimeSpan, ISkillIntervalData>();
                getDataForAllIntervals(DayIntervalGenerator.IntervalForFirstDay(resolution), dayInterval.Value, firstDayBaseDate, intervalToSkillInterval);
                var secondDayBaseDate = new DateOnly(dayInterval.Value.Select(x => x.Period.StartDateTime.Date).Max());
                if (firstDayBaseDate == secondDayBaseDate)
                {
                    twoDayIntervalsForAllDays.Add(secondDayBaseDate, intervalToSkillInterval);
                    continue;
                }
                getDataForAllIntervals(DayIntervalGenerator.IntervalForSecondDay(resolution), dayInterval.Value, secondDayBaseDate, intervalToSkillInterval);
                twoDayIntervalsForAllDays.Add(firstDayBaseDate, intervalToSkillInterval);
            }
            return twoDayIntervalsForAllDays;
        }

        private void getDataForAllIntervals(IEnumerable<TimeSpan> listOfIntervals, IList<ISkillIntervalData> dayIntervalList, DateOnly firstDayBaseDate, Dictionary<TimeSpan, ISkillIntervalData> toSkillInterval)
        {
            foreach (var interval in listOfIntervals)
            {
                TimeSpan interval1 = interval;
                var actualInterval = getTheExactInterval(dayIntervalList, firstDayBaseDate, interval);
                if (actualInterval == null) continue;
                if (new DateOnly(actualInterval.Period.StartDateTime) == firstDayBaseDate &&
                    matchInterval(actualInterval.Period.StartDateTime.TimeOfDay, interval))
                    toSkillInterval.Add(interval1, actualInterval);

            }
        }
        private bool matchInterval(TimeSpan actualInterval, TimeSpan interval)
        {
            var sourceTime = new TimeSpan(interval.Hours, interval.Minutes, interval.Seconds);
            if (sourceTime == actualInterval)
                return true;
            return false;
        }

        private ISkillIntervalData getTheExactInterval(IEnumerable<ISkillIntervalData> skillIntervalDatas, DateTime baseDate, TimeSpan focusedInterval)
        {
            var trimFocusedDate = new TimeSpan(0, focusedInterval.Hours, focusedInterval.Minutes,
                                               focusedInterval.Seconds);
            var extractSkillData = (from eachInterval in skillIntervalDatas
                                    where new DateOnly(eachInterval.Period.StartDateTime) == baseDate
                                    select eachInterval).ToList();
            var returningSkillData = (from eachData in extractSkillData
                                      where eachData.Period.StartDateTime.TimeOfDay == trimFocusedDate
                                      select eachData).FirstOrDefault();
            return returningSkillData;

        }
    }
}
