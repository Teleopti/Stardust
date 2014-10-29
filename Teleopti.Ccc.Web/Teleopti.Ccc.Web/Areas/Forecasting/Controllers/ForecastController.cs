using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage)]
	public class ForecastController : ApiController
	{
		private readonly IQuickForecastForAllSkills _quickForecastForAllSkills;

		public ForecastController(IQuickForecastForAllSkills quickForecastForAllSkills)
		{
			_quickForecastForAllSkills = quickForecastForAllSkills;
		}

		[System.Web.Http.HttpPost, AsyncUnitOfWorkApiAction]
		public Task QuickForecast([FromBody] QuickForecastInputModel model)
		{
			var period = new DateOnlyPeriod(new DateOnly(model.ForecastStart), new DateOnly(model.ForecastEnd));
			_quickForecastForAllSkills.CreateForecast(period);
			return Task.FromResult(true);
		}
	}
}