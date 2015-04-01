namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class ForecastMethodProvider : IForecastMethodProvider
	{
		private readonly IIndexVolumes _indexVolumes;
		private readonly ILinearRegressionTrend _linearRegressionTrend;

		public ForecastMethodProvider(IIndexVolumes indexVolumes, ILinearRegressionTrend linearRegressionTrend)
		{
			_indexVolumes = indexVolumes;
			_linearRegressionTrend = linearRegressionTrend;
		}

		public IForecastMethod[] All()
		{
			return new IForecastMethod[]
			{
				new TeleoptiClassic(_indexVolumes),
				new TeleoptiClassicWithTrend(_indexVolumes, _linearRegressionTrend)
			};
		}
	}
}