using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage)]
	public class ForecastController : ApiController
	{
		[HttpPost]
		public void QuickForecast([FromBody]QuickForecastInputModel model)
		{
			throw new NotImplementedException("This is just a contract for this functionality.");
		}
	}
}