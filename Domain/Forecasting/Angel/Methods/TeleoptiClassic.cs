namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public class TeleoptiClassic : TeleoptiClassicBase
	{
		public TeleoptiClassic(IIndexVolumes indexVolumes):base(indexVolumes)
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassic; }
		}
	}
}