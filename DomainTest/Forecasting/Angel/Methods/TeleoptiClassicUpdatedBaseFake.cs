using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Methods;

namespace Teleopti.Ccc.DomainTest.Forecasting.Angel.Methods
{
	public class TeleoptiClassicUpdatedBaseFake : TeleoptiClassicUpdatedBase
	{
		public TeleoptiClassicUpdatedBaseFake(IIndexVolumes indexVolumes, IAhtAndAcwCalculator ahtAndAcwCalculator):base(indexVolumes, ahtAndAcwCalculator)
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassic; }
		}
	}
}