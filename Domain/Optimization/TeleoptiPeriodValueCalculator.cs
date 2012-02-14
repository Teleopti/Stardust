using System;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
            IPopulationStatisticsCalculator populationStatisticsCalculator =
                new PopulationStatisticsCalculator(true);
            double sum = 0;
            foreach (double? value in _scheduleResultDataExtractor.Values())
            {
                if (value.HasValue)
                {
                    sum += Math.Abs(value.Value);
                    populationStatisticsCalculator.AddItem(value.Value);
                }
            }
            populationStatisticsCalculator.Analyze();
            var stdDev = populationStatisticsCalculator.StandardDeviation;

            return sum + stdDev;
        }
    }
}