using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.OpenForecasterPage)]
	public class ForecastController : ApiController
	{
		private readonly IQuickForecastEvaluator _quickForecastEvaluator;
		private readonly IQuickForecastCreator _quickForecastCreator;
		private readonly ISkillRepository _skillRepository;
		private readonly IPreForecaster _preForecaster;

		public ForecastController(IQuickForecastEvaluator quickForecastEvaluator, IQuickForecastCreator quickForecastCreator, ISkillRepository skillRepository, IPreForecaster preForecaster)
		{
			_quickForecastEvaluator = quickForecastEvaluator;
			_quickForecastCreator = quickForecastCreator;
			_skillRepository = skillRepository;
			_preForecaster = preForecaster;
		}

		[UnitOfWork, Route("api/Forecasting/Skills"), HttpGet]
		public virtual IEnumerable<SkillAccuracy> Skills()
		{
			var skills = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			return skills.Select(
				skill => new SkillAccuracy
				{
					Id = skill.Id.Value,
					Name = skill.Name,
					Workloads = skill.WorkloadCollection.Select(x => new WorkloadAccuracy { Id = x.Id.Value, Name = x.Name }).ToArray()
				});
		}

		[UnitOfWork, HttpGet, Route("api/Forecasting/MeasureForecastMethod")]
		public virtual Task<IEnumerable<SkillAccuracy>> MeasureForecastMethod()
		{
			return Task.FromResult(_quickForecastEvaluator.MeasureForecastForAllSkills());
		}

		[UnitOfWork, HttpPost, Route("api/Forecasting/PreForecast")]
		public virtual Task<WorkloadForecastViewModel> PreForecast(PreForecastInput model)
		{
			return Task.FromResult(_preForecaster.MeasureAndForecast(model));
		}

		[HttpPost, Route("api/Forecasting/Forecast"), UnitOfWork]
		public virtual Task<bool> Forecast(QuickForecastInput model)
		{
			var futurePeriod = new DateOnlyPeriod(new DateOnly(model.ForecastStart), new DateOnly(model.ForecastEnd));
			_quickForecastCreator.CreateForecastForWorkloads(futurePeriod, model.Workloads);
			return Task.FromResult(true);
		}

		[HttpPost, Route("api/Forecasting/ForecastAll"), UnitOfWork]
		public virtual Task<bool> ForecastAll(QuickForecastInput model)
		{
			var futurePeriod = new DateOnlyPeriod(new DateOnly(model.ForecastStart), new DateOnly(model.ForecastEnd));
			_quickForecastCreator.CreateForecastForAll(futurePeriod);
			return Task.FromResult(true);
		}
	}
}