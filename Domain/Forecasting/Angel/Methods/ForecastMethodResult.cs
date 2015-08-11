using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public class ForecastMethodResult
	{
		public IList<IForecastingTarget> ForecastingTargets { get; set; }
	}
}