using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.Domain.Forecasting.Angel.Trend;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public class ForecastMethodProvider : IForecastMethodProvider
	{
		private readonly IIndexVolumes _indexVolumes;
		private readonly ILinearRegressionTrend _linearRegressionTrend;
		private readonly IOutlierRemover _outlierRemover;

		public ForecastMethodProvider(IIndexVolumes indexVolumes, ILinearRegressionTrend linearRegressionTrend, IOutlierRemover outlierRemover)
		{
			_indexVolumes = indexVolumes;
			_linearRegressionTrend = linearRegressionTrend;
			_outlierRemover = outlierRemover;
		}

		public IForecastMethod[] All()
		{
			return new IForecastMethod[]
			{
				new TeleoptiClassic(_indexVolumes, _outlierRemover),
				new TeleoptiClassicWithTrend(_indexVolumes, _linearRegressionTrend, _outlierRemover)
			};
		}

		public IForecastMethod Get(ForecastMethodType forecastMethodType)
		{
			switch (forecastMethodType)
			{
				case ForecastMethodType.TeleoptiClassic:
					return new TeleoptiClassic(_indexVolumes, _outlierRemover);
				case ForecastMethodType.TeleoptiClassicWithTrend:
					return new TeleoptiClassicWithTrend(_indexVolumes, _linearRegressionTrend, _outlierRemover);
				default:
					return null;
			}
		}
	}
}