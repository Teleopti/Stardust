using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    /// <summary>
    /// Calculates the period value by the standard deviation values.
    /// </summary>
    public class StdDevPeriodValueCalculator : IPeriodValueCalculator
    {
        private readonly IScheduleResultDataExtractor _scheduleResultDataExtractor;

        public StdDevPeriodValueCalculator(IScheduleResultDataExtractor scheduleResultDataExtractor)
        {
            _scheduleResultDataExtractor = scheduleResultDataExtractor;}

        public double PeriodValue(IterationOperationOption iterationOperationOption)
        {
			var values = _scheduleResultDataExtractor.Values().Where(v => v.HasValue).Select(v => v.Value);
	        return Calculation.Variances.StandardDeviation(values);
        }
    }
}
