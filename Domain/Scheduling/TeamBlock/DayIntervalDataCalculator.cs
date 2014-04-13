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
		IDictionary<DateTime, ISkillIntervalData> Calculate(IDictionary<DateOnly, IList<ISkillIntervalData>> dayIntervalData);
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

		public IDictionary<DateTime, ISkillIntervalData> Calculate(IDictionary<DateOnly, IList<ISkillIntervalData>> dayIntervalData)
        {
            if (dayIntervalData == null) return null;
	        var sampleInterval = dayIntervalData.Where(x => x.Value.Count > 0).Select(x => x.Value).FirstOrDefault();
			if (!dayIntervalData.Any() || sampleInterval == null)
				return new Dictionary<DateTime, ISkillIntervalData>();

			var resolution = sampleInterval.First().Period.ElapsedTime().TotalMinutes;
            var twoDayIntervalsForAllDays =_twoDaysIntervalGenerator.GenerateTwoDaysInterval( dayIntervalData);

            return _medianCalculatorForDays.CalculateMedian(twoDayIntervalsForAllDays, resolution);;
        }
        
    }
}
