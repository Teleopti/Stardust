using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.Forecasting.Core;
using Teleopti.Ccc.Web.Areas.Forecasting.Models;
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
		private readonly ForecastProvider _forecastProvider;
		private readonly IIntradayPatternViewModelFactory _intradayPatternViewModelFactory;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IAuthorization _authorization;
		private readonly IWorkloadNameBuilder _workloadNameBuilder;
		private readonly IFetchAndFillSkillDays _fetchAndFillSkillDays;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;
		private readonly IIntradayForecaster _intradayForecaster;

		public ForecastController(
			IForecastCreator forecastCreator,
			ISkillRepository skillRepository,
			IForecastViewModelFactory forecastViewModelFactory,
			ForecastProvider forecastProvider,
			IIntradayPatternViewModelFactory intradayPatternViewModelFactory,
			IScenarioRepository scenarioRepository,
			IWorkloadRepository workloadRepository,
			IAuthorization authorization,
			IWorkloadNameBuilder workloadNameBuilder,
			IFetchAndFillSkillDays fetchAndFillSkillDays,
			IHistoricalPeriodProvider historicalPeriodProvider,
			IIntradayForecaster intradayForecaster)
		{
			_forecastCreator = forecastCreator;
			_skillRepository = skillRepository;
			_forecastViewModelFactory = forecastViewModelFactory;
			_forecastProvider = forecastProvider;
			_intradayPatternViewModelFactory = intradayPatternViewModelFactory;
			_scenarioRepository = scenarioRepository;
			_workloadRepository = workloadRepository;
			_authorization = authorization;
			_workloadNameBuilder = workloadNameBuilder;
			_fetchAndFillSkillDays = fetchAndFillSkillDays;
			_historicalPeriodProvider = historicalPeriodProvider;
			_intradayForecaster = intradayForecaster;
		}

		[UnitOfWork, Route("api/Forecasting/Skills"), HttpGet]
		public virtual SkillsViewModel Skills()
		{
			var skillList = _skillRepository.FindSkillsWithAtLeastOneQueueSource();
			return new SkillsViewModel
			{
				IsPermittedToModifySkill = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebModifySkill),
				Skills = skillList.Select(skill => new SkillAccuracy
				{
					Id = skill.Id.Value,
					Workloads = skill.WorkloadCollection.Select(x => new WorkloadAccuracy
					{
						Id = x.Id.Value,
						Name = _workloadNameBuilder.WorkloadName(skill.Name, x.Name)
					}).ToArray()
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

		[HttpPost, Route("api/Forecasting/LoadForecast"), UnitOfWork]
		public virtual IHttpActionResult LoadForecast(ForecastResultInput input)
		{
			var period = new DateOnlyPeriod(new DateOnly(input.ForecastStart), new DateOnly(input.ForecastEnd));
			var scenario = _scenarioRepository.Get(input.ScenarioId);
			return Ok(_forecastProvider.Load(input.WorkloadId, period, scenario));
		}

		[HttpPost, Route("api/Forecasting/EvaluateMethods"), UnitOfWork]
		public virtual Task<WorkloadEvaluateMethodsViewModel> EvaluateMethods(EvaluateMethodsInput input)
		{
			return Task.FromResult(_forecastViewModelFactory.EvaluateMethods(input));
		}

		[HttpPost, Route("api/Forecasting/Forecast"), ReadonlyUnitOfWork]
		public virtual IHttpActionResult Forecast(ForecastInput input)
		{
			var futurePeriod = new DateOnlyPeriod(new DateOnly(input.ForecastStart), new DateOnly(input.ForecastEnd));
			var scenario = _scenarioRepository.Get(input.ScenarioId);
			var forecast = _forecastCreator.CreateForecastForWorkload(futurePeriod, input.Workload, scenario);
			return Ok(forecast);
		}

		[HttpPost, Route("api/Forecasting/IntradayPattern"), UnitOfWork]
		public virtual Task<IntradayPatternViewModel> IntradayPattern(IntradayPatternInput input)
		{
			return Task.FromResult(_intradayPatternViewModelFactory.Create(input));
		}

		[UnitOfWork, HttpPost, Route("api/Forecasting/Campaign")]
		public virtual IHttpActionResult AddCampaign(CampaignInput input)
		{
			foreach (var selectedDay in input.SelectedDays)
			{
				var forecastDay = input.ForecastDays.Single(x => x.Date == selectedDay);
				if (!forecastDay.IsOpen || forecastDay.HasOverride)
				{
					continue;
				}

				forecastDay.HasCampaign = input.CampaignTasksPercent > 0d;
				forecastDay.CampaignTasksPercentage = input.CampaignTasksPercent;
				forecastDay.TotalTasks = (input.CampaignTasksPercent + 1) * forecastDay.Tasks;
			}

			return Ok(input.ForecastDays);
		}

		[UnitOfWork, HttpPost, Route("api/Forecasting/Override")]
		public virtual IHttpActionResult AddOverride(OverrideInput input)
		{
			foreach (var selectedDay in input.SelectedDays)
			{
				var forecastDay = input.ForecastDays.Single(x => x.Date == selectedDay);
				if (!forecastDay.IsOpen)
				{
					continue;
				}

				if (input.ShouldOverrideTasks)
				{
					if (input.OverrideTasks.HasValue)
					{
						forecastDay.TotalTasks = input.OverrideTasks.Value;
					}
					else if (forecastDay.CampaignTasksPercentage > 0)
					{
						forecastDay.TotalTasks = (forecastDay.CampaignTasksPercentage + 1) * forecastDay.Tasks;
					}
					else
					{
						forecastDay.TotalTasks = forecastDay.Tasks;
					}
					forecastDay.OverrideTasks = input.OverrideTasks;
				}

				if (input.ShouldOverrideAverageTaskTime)
				{
					forecastDay.TotalAverageTaskTime = input.OverrideAverageTaskTime ?? forecastDay.AverageTaskTime;
					forecastDay.OverrideAverageTaskTime = input.OverrideAverageTaskTime;
				}

				if (input.ShouldOverrideAverageAfterTaskTime)
				{
					forecastDay.TotalAverageAfterTaskTime = input.OverrideAverageAfterTaskTime ?? forecastDay.AverageAfterTaskTime;
					forecastDay.OverrideAverageAfterTaskTime = input.OverrideAverageAfterTaskTime;
				}

				forecastDay.HasOverride = input.OverrideTasks.HasValue ||
										  input.OverrideAverageTaskTime.HasValue ||
										  input.OverrideAverageAfterTaskTime.HasValue;
				forecastDay.HasCampaign = forecastDay.CampaignTasksPercentage > 0;
			}

			return Ok(input.ForecastDays);
		}

		[UnitOfWork, HttpPost, Route("api/Forecasting/ApplyForecast")]
		public virtual IHttpActionResult ApplyForecast(ForecastModel forecastResult)
		{
			var workload = _workloadRepository.Get(forecastResult.WorkloadId);
			var scenario = _scenarioRepository.Get(forecastResult.ScenarioId);
			var periodStart = new DateOnly(forecastResult.ForecastDays.Min(x => x.Date).Date);
			var periodEnd = new DateOnly(forecastResult.ForecastDays.Max(x => x.Date).Date);
			var forecastPeriod = new DateOnlyPeriod(periodStart, periodEnd);
			var skillDays = _fetchAndFillSkillDays.FindRange(forecastPeriod, workload.Skill, scenario);

			foreach (var skillDay in skillDays)
			{
				var forecastedWorkloadDay =
					skillDay.WorkloadDayCollection.SingleOrDefault(x => x.Workload.Id.Value == forecastResult.WorkloadId);
				if (!forecastedWorkloadDay.OpenForWork.IsOpen)
					continue;
				var model = forecastResult.ForecastDays.SingleOrDefault(x => x.Date == forecastedWorkloadDay.CurrentDate);
				forecastedWorkloadDay.Tasks = model.Tasks;
				forecastedWorkloadDay.AverageTaskTime = TimeSpan.FromSeconds(model.AverageTaskTime);
				forecastedWorkloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(model.AverageAfterTaskTime);
				forecastedWorkloadDay.CampaignTasks = new Percent(model.CampaignTasksPercentage);
				forecastedWorkloadDay.SetOverrideTasks(model.OverrideTasks, new List<ITaskOwner>());
				forecastedWorkloadDay.OverrideAverageTaskTime = model.OverrideAverageTaskTime.HasValue
					? TimeSpan.FromSeconds(model.OverrideAverageTaskTime.Value)
					: (TimeSpan?) null;
				forecastedWorkloadDay.OverrideAverageAfterTaskTime = model.OverrideAverageAfterTaskTime.HasValue
					? TimeSpan.FromSeconds(model.OverrideAverageAfterTaskTime.Value)
					: (TimeSpan?) null;

				var period = _historicalPeriodProvider.AvailablePeriod(workload);
				if (period.HasValue)
				{
					var periodForTemplate = _historicalPeriodProvider.AvailableIntradayTemplatePeriod(period.Value);
					_intradayForecaster.Apply(workload, periodForTemplate, skillDay.WorkloadDayCollection);
				}
			}

			return Ok();
		}
	}
}