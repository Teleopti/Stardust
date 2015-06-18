namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public interface IAhtAndAcwCalculator
	{
		AhtAndAcw Recent3MonthsAverage(ITaskOwnerPeriod historicalData);
	}
}