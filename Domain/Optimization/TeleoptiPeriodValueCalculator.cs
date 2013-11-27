using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class TeleoptiPeriodValueCalculator : IPeriodValueCalculator
    {
        private readonly IScheduleResultDataExtractor _scheduleResultDataExtractor;

        public TeleoptiPeriodValueCalculator(IScheduleResultDataExtractor scheduleResultDataExtractor)
        {
            _scheduleResultDataExtractor = scheduleResultDataExtractor;
        }

        public double PeriodValue(IterationOperationOption iterationOperationOption)
        {
	        var values = _scheduleResultDataExtractor.Values().Where(v => v.HasValue).Select(v => v.Value);
	        return Calculation.Variances.Teleopti(values);
        }
    }
}