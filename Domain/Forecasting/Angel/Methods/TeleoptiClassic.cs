namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public class TeleoptiClassic : TeleoptiClassicUpdatedBase
	{
		public TeleoptiClassic(IIndexVolumes indexVolumes)
			: base(indexVolumes, new SimpleAhtAndAcwCalculator())
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassic; }
		}
	}
}