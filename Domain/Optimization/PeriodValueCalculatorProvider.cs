using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IPeriodValueCalculatorProvider
    {
        IPeriodValueCalculator CreatePeriodValueCalculator(IOptimizerOriginalPreferences optimizerPreferences, IScheduleResultDataExtractor dataExtractor);
    }

    public class PeriodValueCalculatorProvider : IPeriodValueCalculatorProvider
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public IPeriodValueCalculator CreatePeriodValueCalculator(IOptimizerOriginalPreferences optimizerPreferences, IScheduleResultDataExtractor dataExtractor)
        {
            if (optimizerPreferences.AdvancedPreferences.UseStandardDeviationCalculation)
                return new StdDevPeriodValueCalculator(dataExtractor);
            if(optimizerPreferences.AdvancedPreferences.UseTeleoptiCalculation)
                return new TeleoptiPeriodValueCalculator(dataExtractor);
            return new RmsPeriodValueCalculator(dataExtractor);
        }
    }
}
