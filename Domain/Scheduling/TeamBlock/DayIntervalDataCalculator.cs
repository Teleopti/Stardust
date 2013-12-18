using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IDayIntervalDataCalculator
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        IDictionary<TimeSpan, ISkillIntervalData> Calculate(IDictionary<DateOnly, IList<ISkillIntervalData>> dayIntervalData);
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

        public IDictionary<TimeSpan, ISkillIntervalData> Calculate(IDictionary<DateOnly, IList<ISkillIntervalData>> dayIntervalData)
        {
            if (dayIntervalData == null) return null;
	        var resolution = dayIntervalData.First().Value[0].Period.ElapsedTime().TotalMinutes;
            var twoDayIntervalsForAllDays =_twoDaysIntervalGenerator.GenerateTwoDaysInterval( dayIntervalData, (int)resolution);

            return _medianCalculatorForDays.CalculateMedian(twoDayIntervalsForAllDays, resolution);;
        }
        
    }
}
