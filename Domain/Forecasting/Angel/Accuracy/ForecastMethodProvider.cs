namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public class ForecastMethodProvider : IForecastMethodProvider
	{
		private readonly IIndexVolumes _indexVolumes;

		public ForecastMethodProvider(IIndexVolumes indexVolumes)
		{
			_indexVolumes = indexVolumes;
		}

		public IForecastMethod[] All()
		{
			return new IForecastMethod[] {new TeleoptiClassic(_indexVolumes)};
		}
	}
}