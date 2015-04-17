using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Web.Areas.Forecasting.Controllers;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public interface IPreForecaster
	{
		WorkloadForecastViewModel MeasureAndForecast(PreForecastInput model);
	}
}