using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Ccc.Web.Areas.Global;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebForecasts)]
	public class ForecastController : ApiController
	{
		private readonly IForecastCreator _forecastCreator;
		private readonly ISkillRepository _skillRepository;
		private readonly IForecastViewModelFactory _forecastViewModelFactory;
		private readonly IForecastResultViewModelFactory _forecastResultViewModelFactory;
		private readonly IIntradayPatternViewModelFactory _intradayPatternViewModelFactory;
		private readonly IActionThrottler _actionThrottler;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly ICampaignPersister _campaignPersister;
		private readonly IOverridePersister _overridePersister;
		private readonly IAuthorization _authorization;
		private readonly IForecastMisc _forecastMisc;

		public ForecastController(
			IForecastCreator forecastCreator,
			ISkillRepository skillRepository,
			IForecastViewModelFactory forecastViewModelFactory,
			IForecastResultViewModelFactory forecastResultViewModelFactory,
			IIntradayPatternViewModelFactory intradayPatternViewModelFactory, 
			IActionThrottler actionThrottler,
			IScenarioRepository scenarioRepository, 
			IWorkloadRepository workloadRepository,
			ICampaignPersister campaignPersister,
			IOverridePersister overridePersister,
			IAuthorization authorization,
			IForecastMisc forecastMisc)
		{
			_forecastCreator = forecastCreator;
			_skillRepository = skillRepository;
			_forecastViewModelFactory = forecastViewModelFactory;
			_forecastResultViewModelFactory = forecastResultViewModelFactory;
			_intradayPatternViewModelFactory = intradayPatternViewModelFactory;
			_actionThrottler = actionThrottler;
			_scenarioRepository = scenarioRepository;
			_workloadRepository = workloadRepository;
			_campaignPersister = campaignPersister;
			_overridePersister = overridePersister;
			_authorization = authorization;
			_forecastMisc = forecastMisc;
		}

		[UnitOfWork, Route("api/Forecasting/Skills"), HttpGet]
		public virtual SkillsViewModel Skills()
		{
			var skillList = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			return new SkillsViewModel
			{
				IsPermittedToModifySkill = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebModifySkill),
				Skills = skillList.Select(
					skill => new SkillAccuracy
					{
						Id = skill.Id.Value,
						Workloads =
							skill.WorkloadCollection.Select(
								x => new WorkloadAccuracy {Id = x.Id.Value, Name = _forecastMisc.WorkloadName(skill.Name, x.Name)}).ToArray()
					})
			};
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
			var futurePeriod = new DateOnlyPeriod(new DateOnly(input.ForecastStart), new DateOnly(input.ForecastEnd));
			_forecastCreator.CreateForecastForWorkloads(futurePeriod, input.Workloads,
				_scenarioRepository.Get(input.ScenarioId));
			return Task.FromResult(new ForecastResultViewModel
			{
				Success = true
			});
		}

		[HttpPost, Route("api/Forecasting/IntradayPattern"), UnitOfWork]
		public virtual Task<IntradayPatternViewModel> IntradayPattern(IntradayPatternInput input)
		{
			return Task.FromResult(_intradayPatternViewModelFactory.Create(input));
		}

		[UnitOfWork, HttpPost, Route("api/Forecasting/Campaign")]
		public virtual Task<ForecastResultViewModel> AddCampaign(CampaignInput input)
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
				var scenario = _scenarioRepository.Get(input.ScenarioId);
				var workload = _workloadRepository.Get(input.WorkloadId);
				_campaignPersister.Persist(scenario, workload, input.Days, input.CampaignTasksPercent);
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

		[UnitOfWork, HttpPost, Route("api/Forecasting/Override")]
		public virtual Task<ForecastResultViewModel> Override(OverrideInput input)
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
				var scenario = _scenarioRepository.Get(input.ScenarioId);
				var workload = _workloadRepository.Get(input.WorkloadId);
				_overridePersister.Persist(scenario, workload, input);
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
	}

	public class SkillsViewModel
	{
		public IEnumerable<SkillAccuracy> Skills { get; set; }
		public bool IsPermittedToModifySkill { get; set; }
	}

	public class ForecastResultViewModel
	{
		public bool Success { get; set; }
		public string Message { get; set; }
	}

	public class ModifyInput
	{
		public ModifiedDay[] Days { get; set; }
		public Guid ScenarioId { get; set; }
		public Guid WorkloadId { get; set; }
	}

	public class CampaignInput : ModifyInput
	{
		public double CampaignTasksPercent { get; set; }
	}

	public class OverrideInput : ModifyInput
	{
		public double? OverrideTasks { get; set; }
		public double? OverrideTalkTime { get; set; }
		public double? OverrideAfterCallWork { get; set; }
		public bool ShouldSetOverrideTasks { get; set; }
		public bool ShouldSetOverrideTalkTime { get; set; }
		public bool ShouldSetOverrideAfterCallWork { get; set; }
	}

	public class ModifiedDay
	{
		public DateTime Date { get; set; }
	}
}