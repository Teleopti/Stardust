using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Methods
{
	public class TeleoptiClassicBaseFake : TeleoptiClassicBase
	{
		public TeleoptiClassicBaseFake(IIndexVolumes indexVolumes) : base(indexVolumes)
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassicLongTerm; }
		}
	}
}