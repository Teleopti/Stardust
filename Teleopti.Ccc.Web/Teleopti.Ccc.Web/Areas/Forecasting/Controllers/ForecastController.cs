using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
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
		private readonly ICurrentIdentity _currentIdentity;
		private readonly INow _now;

		public ForecastController(IQuickForecastForAllSkills quickForecastForAllSkills, IForecastHistoricalPeriodCalculator forecastHistoricalPeriodCalculator, ICurrentIdentity currentIdentity, INow now)
		{
			_quickForecastForAllSkills = quickForecastForAllSkills;
			_forecastHistoricalPeriodCalculator = forecastHistoricalPeriodCalculator;
			_currentIdentity = currentIdentity;
			_now = now;
		}

		public object GetThatShouldBeInAMoreGenericControllerLaterOn()
		{
			return new {UserName = _currentIdentity.Current().Name};
		}

		[HttpPost, UnitOfWork]
		public virtual Task QuickForecast([FromBody] QuickForecastInputModel model)
		{
			var futurePeriod = new DateOnlyPeriod(new DateOnly(model.ForecastStart), new DateOnly(model.ForecastEnd));
			var historicalPeriod = _forecastHistoricalPeriodCalculator.HistoricalPeriod(futurePeriod);

			_quickForecastForAllSkills.CreateForecast(historicalPeriod, futurePeriod);
			return Task.FromResult(true);
		}

		[HttpGet, UnitOfWork, Route("api/Forecast/Measure")]
		public virtual double MeasureForecast()
		{
			var yesterday = _now.UtcDateTime().AddDays(-1);
			var futurePeriod = new DateOnlyPeriod(new DateOnly(yesterday.AddYears(-1)), new DateOnly(yesterday));
			var historicalPeriod = _forecastHistoricalPeriodCalculator.OneYearHistoricalPeriod(futurePeriod);

			return _quickForecastForAllSkills.MeasureForecast(historicalPeriod, futurePeriod);
		}
	}
}