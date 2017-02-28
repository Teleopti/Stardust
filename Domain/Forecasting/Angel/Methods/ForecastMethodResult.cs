using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public class ForecastMethodResult
	{
		public IList<IForecastingTarget> ForecastingTargets { get; set; }
	}
}