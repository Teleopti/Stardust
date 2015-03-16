namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public interface IQuickForecasterWorkload
	{
		double Execute(QuickForecasterWorkloadParams quickForecasterWorkloadParams);
	}
}