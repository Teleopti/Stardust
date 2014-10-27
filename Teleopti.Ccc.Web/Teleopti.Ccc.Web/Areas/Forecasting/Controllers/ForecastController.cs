using System;
using System.Web.Http;
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

		public ForecastController(IQuickForecastForAllSkills quickForecastForAllSkills)
		{
			_quickForecastForAllSkills = quickForecastForAllSkills;
		}

		[HttpPost, UnitOfWorkApiAction]
		public void QuickForecast([FromBody]QuickForecastInputModel model)
		{
			var period = new DateOnlyPeriod(new DateOnly(model.ForecastStart), new DateOnly(model.ForecastEnd));
			_quickForecastForAllSkills.CreateForecast(period);
		}
	}
}