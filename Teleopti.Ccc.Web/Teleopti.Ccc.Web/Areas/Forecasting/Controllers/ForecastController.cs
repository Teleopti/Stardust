using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Ccc.Web.Areas.Global;
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
		private readonly IIntradayPatternViewModelFactory _intradayPatternViewModelFactory;
		private readonly IActionThrottler _actionThrottler;
		private readonly IScenarioRepository _scenarioRepository;

		public ForecastController(IForecastCreator forecastCreator, ISkillRepository skillRepository, IForecastViewModelFactory forecastViewModelFactory, IForecastResultViewModelFactory forecastResultViewModelFactory, IIntradayPatternViewModelFactory intradayPatternViewModelFactory, IActionThrottler actionThrottler, IScenarioRepository scenarioRepository)
		{
			_forecastCreator = forecastCreator;
			_skillRepository = skillRepository;
			_forecastViewModelFactory = forecastViewModelFactory;
			_forecastResultViewModelFactory = forecastResultViewModelFactory;
			_intradayPatternViewModelFactory = intradayPatternViewModelFactory;
			_actionThrottler = actionThrottler;
			_scenarioRepository = scenarioRepository;
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

		[UnitOfWork, Route("api/Forecasting/Scenarios"), HttpGet]
		public virtual IEnumerable<ScenarioViewModel> Scenarios()
		{
			var scenarios = _scenarioRepository.FindAllSorted();
			return scenarios.Select(x => new ScenarioViewModel
			{
				Id = x.Id.GetValueOrDefault(),
				Name = x.Description.Name
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
			return
				Task.FromResult(_forecastResultViewModelFactory.Create(input.WorkloadId,
					new DateOnlyPeriod(new DateOnly(input.ForecastStart), new DateOnly(input.ForecastEnd)),
					_scenarioRepository.Get(input.ScenarioId)));
		}

		[HttpPost, Route("api/Forecasting/EvaluateMethods"), UnitOfWork]
		public virtual Task<WorkloadEvaluateMethodsViewModel> EvaluateMethods(EvaluateMethodsInput input)
		{
			return Task.FromResult(_forecastViewModelFactory.EvaluateMethods(input));
		}

		[HttpPost, Route("api/Forecasting/Forecast"), UnitOfWork]
		public virtual Task<ForecastResultViewModel> Forecast(ForecastInput input)
		{
			var failedTask = Task.FromResult(new ForecastResultViewModel
			{
				Success = false,
				Message = "Forecast is running, please try later."
			});
			if (_actionThrottler.IsBlocked(ThrottledAction.Forecasting))
			{
				return failedTask;
			}
			var token = _actionThrottler.Block(ThrottledAction.Forecasting);
			try
			{
				var futurePeriod = new DateOnlyPeriod(new DateOnly(input.ForecastStart), new DateOnly(input.ForecastEnd));
				_forecastCreator.CreateForecastForWorkloads(futurePeriod, input.Workloads);
				return Task.FromResult(new ForecastResultViewModel
				{
					Success = true
				});
			}
			catch (OptimisticLockException)
			{
				return failedTask;
			}
			finally
			{
				_actionThrottler.Finish(token);
			}
		}

		[HttpPost, Route("api/Forecasting/IntradayPattern"), UnitOfWork]
		public virtual Task<IntradayPatternViewModel> IntradayPattern(IntradayPatternInput input)
		{
			return Task.FromResult(_intradayPatternViewModelFactory.Create(input));
		}
	}

	public class ScenarioViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}

	public class ForecastResultViewModel
	{
		public bool Success { get; set; }
		public string Message { get; set; }
	}
}