using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IPeriodValueCalculatorProvider
    {
        IPeriodValueCalculator CreatePeriodValueCalculator(IAdvancedPreferences advancedPreferences, IScheduleResultDataExtractor dataExtractor);
    }

    public class PeriodValueCalculatorProvider : IPeriodValueCalculatorProvider
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public IPeriodValueCalculator CreatePeriodValueCalculator(IAdvancedPreferences advancedPreferences, IScheduleResultDataExtractor dataExtractor)
        {
            TargetValueOptions targetValueOptions = advancedPreferences.TargetValueCalculation;

            switch (targetValueOptions)
            {
                case TargetValueOptions.StandardDeviation:
                    return new StdDevPeriodValueCalculator(dataExtractor);
                case TargetValueOptions.Teleopti:
                    return new TeleoptiPeriodValueCalculator(dataExtractor);
                default:
                    return new RmsPeriodValueCalculator(dataExtractor);
            }
        }
    }
}
