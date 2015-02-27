using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
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
		private readonly ICurrentIdentity _currentIdentity;

		public ForecastController(IQuickForecastForAllSkills quickForecastForAllSkills, ICurrentIdentity currentIdentity)
		{
			_quickForecastForAllSkills = quickForecastForAllSkills;
			_currentIdentity = currentIdentity;
		}

		public object GetThatShouldBeInAMoreGenericControllerLaterOn()
		{
			return new {UserName = _currentIdentity.Current().Name};
		}

		[HttpPost, UnitOfWork]
		public virtual Task<double> QuickForecast([FromBody] QuickForecastInputModel model)
		{
			var futurePeriod = new DateOnlyPeriod(new DateOnly(model.ForecastStart), new DateOnly(model.ForecastEnd));

			var measureResult = _quickForecastForAllSkills.CreateForecast(futurePeriod);
			return Task.FromResult(measureResult);
		}
	}
}