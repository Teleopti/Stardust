namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public class TeleoptiClassicLongTerm : TeleoptiClassicBase
	{
		public TeleoptiClassicLongTerm(IDayWeekMonthIndexVolumes indexVolumes)
			: base(indexVolumes)
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassic; }
		}
	}
}