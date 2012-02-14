using Teleopti.Ccc.Domain.ResourceCalculation;
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
            IPopulationStatisticsCalculator populationStatisticsCalculator =
                new PopulationStatisticsCalculator(true);
            foreach (double? value in _scheduleResultDataExtractor.Values())
            {
                if (value.HasValue)
                    populationStatisticsCalculator.AddItem(value.Value);
            }
            populationStatisticsCalculator.Analyze();
            return populationStatisticsCalculator.RootMeanSquare;
        }
    }
}
