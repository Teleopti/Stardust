namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class ForecastMethodProvider : IForecastMethodProvider
	{
		private readonly IndexVolumes _indexVolumes;

		public ForecastMethodProvider(IndexVolumes indexVolumes)
		{
			_indexVolumes = indexVolumes;
		}

		public IForecastMethod[] All()
		{
			return new IForecastMethod[] {new TeleoptiClassic(_indexVolumes)};
		}
	}
}