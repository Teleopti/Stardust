using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;

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

		public IForecastMethod Get(ForecastMethodType forecastMethodType)
		{
			switch (forecastMethodType)
			{
				case ForecastMethodType.TeleoptiClassic:
					return new TeleoptiClassic(_indexVolumes);
				case ForecastMethodType.TeleoptiClassicWithTrend:
					return new TeleoptiClassicWithTrend(_indexVolumes, _linearRegressionTrend);
				default:
					return null;
			}
		}
	}
}