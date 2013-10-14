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
        IDictionary<TimeSpan, ISkillIntervalData> Calculate(int resolution, IDictionary<DateOnly, IList<ISkillIntervalData>> dayIntervalData);
    }

    public class DayIntervalDataCalculator : IDayIntervalDataCalculator
    {
        private readonly IMedianCalculatorForDays _medianCalculatorForDays;
        private readonly ITwoDaysIntervalGenerator _twoDaysIntervalGenerator;

        public DayIntervalDataCalculator(IMedianCalculatorForDays medianCalculatorForDays, ITwoDaysIntervalGenerator twoDaysIntervalGenerator )
        {
            _medianCalculatorForDays = medianCalculatorForDays;
            _twoDaysIntervalGenerator = twoDaysIntervalGenerator;
        }

        public IDictionary<TimeSpan, ISkillIntervalData> Calculate(int resolution, IDictionary<DateOnly,
            IList<ISkillIntervalData>> dayIntervalData)
        {
            if (dayIntervalData == null) return null;
            InParameter.ValueMustBeLargerThanZero("resolution", resolution);
            var baseDate = DateTime.SpecifyKind(SkillDayTemplate.BaseDate, DateTimeKind.Utc);
            var twoDayIntervalsForAllDays =_twoDaysIntervalGenerator.GenerateTwoDaysInterval( dayIntervalData, resolution);
            var result = new Dictionary<TimeSpan, ISkillIntervalData>();

            var temp = _medianCalculatorForDays.CalculateMedian(twoDayIntervalsForAllDays, resolution);

            //adding the missing interval which should not be included in median calculation
            foreach (var interval in DayIntervalGenerator.IntervalForTwoDays(resolution))
            {
                if (!temp.ContainsKey(interval))
                {
                    var startTime = baseDate.Date.Add(interval);
                    var endTime = startTime.AddMinutes(resolution);
                    result.Add(interval, new SkillIntervalData(new DateTimePeriod(startTime, endTime), 0, 0, 0,null, null));
                }
                else
                {
                    result.Add(interval, temp[interval]);
                }
            }

            return result;
        }
        
    }
}
