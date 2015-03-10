using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
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
		private readonly IQuickForecastCreator _quickForecastCreator;
		private readonly ICurrentIdentity _currentIdentity;

		public ForecastController(IQuickForecastCreator quickForecastCreator, ICurrentIdentity currentIdentity)
		{
			_quickForecastCreator = quickForecastCreator;
			_currentIdentity = currentIdentity;
		}

		public object GetThatShouldBeInAMoreGenericControllerLaterOn()
		{
			return new {UserName = _currentIdentity.Current().Name};
		}

		[HttpPost, UnitOfWork]
		public virtual Task<ForecastingAccuracy[]> QuickForecast([FromBody] QuickForecastInputModel model)
		{
			var futurePeriod = new DateOnlyPeriod(new DateOnly(model.ForecastStart), new DateOnly(model.ForecastEnd));
			if (model.Workloads.IsNullOrEmpty())
			{
				return Task.FromResult(new[] { _quickForecastCreator.CreateForecastForAllSkills(futurePeriod) });
			}
			return Task.FromResult( _quickForecastCreator.CreateForecastForWorkloads(futurePeriod, model.Workloads));
		}
	}
}