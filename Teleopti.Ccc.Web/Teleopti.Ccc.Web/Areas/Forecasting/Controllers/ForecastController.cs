using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage)]
	public class ForecastController : ApiController
	{
		private readonly IQuickForecastForAllSkills _quickForecastForAllSkills;
		private readonly IForecastHistoricalPeriodCalculator _forecastHistoricalPeriodCalculator;

		public ForecastController(IQuickForecastForAllSkills quickForecastForAllSkills, 
														IForecastHistoricalPeriodCalculator forecastHistoricalPeriodCalculator)
		{
			_quickForecastForAllSkills = quickForecastForAllSkills;
			_forecastHistoricalPeriodCalculator = forecastHistoricalPeriodCalculator;
		}

		[HttpPost, AsyncUnitOfWorkApiAction]
		public Task QuickForecast([FromBody] QuickForecastInputModel model)
		{
			var futurePeriod = new DateOnlyPeriod(new DateOnly(model.ForecastStart), new DateOnly(model.ForecastEnd));
			var historicalPeriod = _forecastHistoricalPeriodCalculator.HistoricalPeriod(futurePeriod);

			_quickForecastForAllSkills.CreateForecast(historicalPeriod, futurePeriod);
			return Task.FromResult(true);
		}
	}
}