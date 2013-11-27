using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Calculates the period value by the daily rms values.
    /// </summary>
    public class RmsPeriodValueCalculator : IPeriodValueCalculator
    {
        private readonly IScheduleResultDataExtractor _scheduleResultDataExtractor;

        public RmsPeriodValueCalculator(IScheduleResultDataExtractor scheduleResultDataExtractor)
        {
            _scheduleResultDataExtractor = scheduleResultDataExtractor;}

        public double PeriodValue(IterationOperationOption iterationOperationOption)
        {
			var values = _scheduleResultDataExtractor.Values().Where(v => v.HasValue).Select(v => v.Value);
            return Calculation.Variances.RMS(values);
        }
    }
}
