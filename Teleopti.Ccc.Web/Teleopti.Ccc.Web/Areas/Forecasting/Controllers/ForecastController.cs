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
		private readonly IForecastCreator _forecastCreator;
		private readonly ISkillRepository _skillRepository;
		private readonly IForecastViewModelFactory _forecastViewModelFactory;
		private readonly IForecastResultViewModelFactory _forecastResultViewModelFactory;

		public ForecastController(IForecastCreator forecastCreator, ISkillRepository skillRepository, IForecastViewModelFactory forecastViewModelFactory, IForecastResultViewModelFactory forecastResultViewModelFactory)
		{
			_forecastCreator = forecastCreator;
			_skillRepository = skillRepository;
			_forecastViewModelFactory = forecastViewModelFactory;
			_forecastResultViewModelFactory = forecastResultViewModelFactory;
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

		[UnitOfWork, HttpPost, Route("api/Forecasting/Evaluate")]
		public virtual Task<WorkloadEvaluateViewModel> Evaluate(EvaluateInput input)
		{
			return Task.FromResult(_forecastViewModelFactory.Evaluate(input));
		}

		[UnitOfWork, HttpPost, Route("api/Forecasting/QueueStatistics")]
		public virtual Task<WorkloadQueueStatisticsViewModel> QueueStatistics(QueueStatisticsInput input)
		{
			return Task.FromResult(_forecastViewModelFactory.QueueStatistics(input));
		}

		[HttpPost, Route("api/Forecasting/ForecastResult"), UnitOfWork]
		public virtual Task<WorkloadForecastResultViewModel> ForecastResult(ForecastResultInput input)
		{
			return Task.FromResult(_forecastResultViewModelFactory.Create(input.WorkloadId, new DateOnlyPeriod(new DateOnly(input.ForecastStart), new DateOnly(input.ForecastEnd))));
		}

		[HttpPost, Route("api/Forecasting/EvaluateDev"), UnitOfWork]
		public virtual Task<WorkloadEvaluateDevViewModel> EvaluateDev(EvaluateDevInput input)
		{
			return Task.FromResult(_forecastViewModelFactory.EvaluateDev(input));
		}

		[HttpPost, Route("api/Forecasting/Forecast"), UnitOfWork]
		public virtual Task<bool> Forecast(ForecastInput input)
		{
			var futurePeriod = new DateOnlyPeriod(new DateOnly(input.ForecastStart), new DateOnly(input.ForecastEnd));
			_forecastCreator.CreateForecastForWorkloads(futurePeriod, input.Workloads);
			return Task.FromResult(true);
		}
	}

}