using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IDayIntervalDataCalculator
	{
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IDictionary<TimeSpan, ISkillIntervalData> CalculatePerfect(int resolution, IDictionary<DateOnly, IList<ISkillIntervalData>> dayIntervalData);
	}

	public class DayIntervalDataCalculator : IDayIntervalDataCalculator
	{
		private readonly IMedianCalculatorForDays _medianCalculatorForDays;

        public DayIntervalDataCalculator(IMedianCalculatorForDays medianCalculatorForDays)
        {
            _medianCalculatorForDays = medianCalculatorForDays;
        }

        public IDictionary<TimeSpan, ISkillIntervalData> CalculatePerfect(int resolution, IDictionary<DateOnly,
            IList<ISkillIntervalData>> dayIntervalData)
        {
            if (dayIntervalData == null) return null;
            InParameter.ValueMustBeLargerThanZero("resolution", resolution);
            var baseDate = DateTime.SpecifyKind(SkillDayTemplate.BaseDate, DateTimeKind.Utc);
            var twoDayIntervalsForAllDays =  getTwoDaysIntervalForAllDays(dayIntervalData, resolution  );
            var result = new Dictionary<TimeSpan, ISkillIntervalData>();

            var temp = _medianCalculatorForDays.CalculateMedian(twoDayIntervalsForAllDays,resolution );

            //adding the missing interval which should not be included in median calculation
            foreach (var interval in DayIntervalGenerator.IntervalForTwoDays(resolution))
            {
                if (!temp.ContainsKey(interval))
                {
                    var startTime = baseDate.Date.Add(interval);
                    var endTime = startTime.AddMinutes(resolution);
                    result .Add(interval, new SkillIntervalData(new DateTimePeriod(startTime, endTime), 0, 0, 0, 0, 0));
                }
                else
                {
                    result.Add(interval,temp[interval ]);
                }
            }
            
            return result;
        }

        private ISkillIntervalData getTheExactInterval(IEnumerable<ISkillIntervalData> skillIntervalDatas, DateTime baseDate, TimeSpan focusedInterval )
        {
            var trimFocusedDate = new TimeSpan(0, focusedInterval.Hours, focusedInterval.Minutes,
                                               focusedInterval.Seconds);
            var extractSkillData = (from eachInterval in skillIntervalDatas
                                    where new DateOnly(eachInterval.Period.StartDateTime) == baseDate
                                    select eachInterval).ToList() ;
            var returningSkillData = (from eachData in extractSkillData
                                      where eachData.Period.StartDateTime.TimeOfDay == trimFocusedDate
                                      select eachData).FirstOrDefault();
            return returningSkillData;

        }

        private Dictionary<DateOnly, Dictionary<TimeSpan, ISkillIntervalData>> getTwoDaysIntervalForAllDays(IEnumerable<KeyValuePair<DateOnly, IList<ISkillIntervalData>>> dayIntervalData,  int resolution)
	    {
            var twoDayIntervalsForAllDays = new Dictionary<DateOnly, Dictionary<TimeSpan, ISkillIntervalData>>();

            foreach (var dayInterval in dayIntervalData)
	        {
	            var firstDayBaseDate = new DateOnly(dayInterval.Value.Select(x => x.Period.StartDateTime.Date).Min());
                var intervalToSkillInterval = new Dictionary<TimeSpan, ISkillIntervalData>();
                getDataForAllIntervals(DayIntervalGenerator.IntervalForFirstDay( resolution ), dayInterval.Value, firstDayBaseDate, resolution, intervalToSkillInterval);
                var secondDayBaseDate = new DateOnly(dayInterval.Value.Select(x => x.Period.StartDateTime.Date).Max( ));
                if (firstDayBaseDate == secondDayBaseDate)
                {
                    twoDayIntervalsForAllDays.Add(secondDayBaseDate, intervalToSkillInterval);
                    continue;
                }
                getDataForAllIntervals(DayIntervalGenerator.IntervalForSecondDay( resolution), dayInterval.Value, secondDayBaseDate, resolution, intervalToSkillInterval);
                twoDayIntervalsForAllDays.Add(firstDayBaseDate, intervalToSkillInterval);
	        }
	        return twoDayIntervalsForAllDays;
	    }

	    private void getDataForAllIntervals(IEnumerable<TimeSpan> listOfIntervals, IList<ISkillIntervalData> dayIntervalList, DateOnly firstDayBaseDate, int resolution, Dictionary<TimeSpan, ISkillIntervalData> toSkillInterval)
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

	    
	}
}
