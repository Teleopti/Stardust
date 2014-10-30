using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage)]
	public class ForecastController : ApiController
	{
		private readonly IQuickForecastForAllSkills _quickForecastForAllSkills;
		private readonly INow _now;

		public ForecastController(IQuickForecastForAllSkills quickForecastForAllSkills, INow now)
		{
			_quickForecastForAllSkills = quickForecastForAllSkills;
			_now = now;
		}

		[HttpPost, AsyncUnitOfWorkApiAction]
		public Task QuickForecast([FromBody] QuickForecastInputModel model)
		{
			var nowDate = _now.LocalDateOnly();
			var futurePeriod = new DateOnlyPeriod(new DateOnly(model.ForecastStart), new DateOnly(model.ForecastEnd));
			var historicalPeriodStartTime = new DateOnly(nowDate.Date.AddYears(-1));
			var historicalPeriod = new DateOnlyPeriod(historicalPeriodStartTime, new DateOnly(nowDate));
			_quickForecastForAllSkills.CreateForecast(historicalPeriod, futurePeriod);
			return Task.FromResult(true);
		}
	}
}