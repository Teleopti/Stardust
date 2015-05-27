using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public class TeleoptiClassic : TeleoptiClassicBase
	{
		public TeleoptiClassic(IIndexVolumes indexVolumes, IOutlierRemover outlierRemover)
			: base(indexVolumes, outlierRemover)
		{
		}

		public override ForecastMethodType Id
		{
			get { return ForecastMethodType.TeleoptiClassic; }
		}
	}
}