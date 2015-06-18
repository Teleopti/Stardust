using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Methods
{
	public class TeleoptiClassicBaseFake : TeleoptiClassicBase
	{
		public TeleoptiClassicBaseFake(IDayWeekMonthIndexVolumes indexVolumes):base(indexVolumes)
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassic; }
		}
	}
}