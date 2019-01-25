using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios
{
	public class PeriodValueCalculatorProviderThatReturnsCalculatorThatReturnsWrongValue : IPeriodValueCalculatorProvider
	{
		public IPeriodValueCalculator CreatePeriodValueCalculator(IAdvancedPreferences advancedPreferences, IScheduleResultDataExtractor dataExtractor)
		{
			return new periodValueCalculatorReturningOppositeValue(new StdDevPeriodValueCalculator(dataExtractor));
		}

		private class periodValueCalculatorReturningOppositeValue : IPeriodValueCalculator
		{
			private readonly StdDevPeriodValueCalculator _periodValueCalculator;

			public periodValueCalculatorReturningOppositeValue(StdDevPeriodValueCalculator periodValueCalculator)
			{
				_periodValueCalculator = periodValueCalculator;
			}

			public double PeriodValue(IterationOperationOption iterationOperationOption)
			{
				return -_periodValueCalculator.PeriodValue(iterationOperationOption);
			}
		}
	}
}