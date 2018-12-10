using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Forecasting.Models;
using Teleopti.Ccc.Web.Filters;


namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebForecasts)]
	public class ForecastController : ApiController
	{
		private const double tolerance = 0.000001d;

		private readonly ForecastViewModelCreator _forecastViewModelCreator;
		private readonly ISkillRepository _skillRepository;
		private readonly ForecastProvider _forecastProvider;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IAuthorization _authorization;
		private readonly IFetchAndFillSkillDays _fetchAndFillSkillDays;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;
		private readonly IntradayForecaster _intradayForecaster;
		private readonly IForecastDayOverrideRepository _forecastDayOverrideRepository;
		private readonly IQueueStatisticsViewModelFactory _queueStatisticsViewModelFactory;
		private readonly ILoadStatistics _loadStatistics;

		public ForecastController(
			ForecastViewModelCreator quickForecaster,
			ISkillRepository skillRepository,
			ForecastProvider forecastProvider,
			IScenarioRepository scenarioRepository,
			IWorkloadRepository workloadRepository,
			IAuthorization authorization,
			IFetchAndFillSkillDays fetchAndFillSkillDays,
			IHistoricalPeriodProvider historicalPeriodProvider,
			IntradayForecaster intradayForecaster,
			IForecastDayOverrideRepository forecastDayOverrideRepository,
			IQueueStatisticsViewModelFactory queueStatisticsViewModelFactory,
			ILoadStatistics loadStatistics)
		{
			_forecastViewModelCreator = quickForecaster;
			_skillRepository = skillRepository;
			_forecastProvider = forecastProvider;
			_scenarioRepository = scenarioRepository;
			_workloadRepository = workloadRepository;
			_authorization = authorization;
			_fetchAndFillSkillDays = fetchAndFillSkillDays;
			_historicalPeriodProvider = historicalPeriodProvider;
			_intradayForecaster = intradayForecaster;
			_forecastDayOverrideRepository = forecastDayOverrideRepository;
			_queueStatisticsViewModelFactory = queueStatisticsViewModelFactory;
			_loadStatistics = loadStatistics;
		}

		[UnitOfWork, Route("api/Forecasting/Skills"), HttpGet]
		public virtual IHttpActionResult Skills()
		{
			var skillList = _skillRepository.FindSkillsWithAtLeastOneQueueSource().ToList();
			var skillTypes = skillList.ToDictionary(x => x.Id, x => x.SkillType.Description.Name);
			return Ok(new
			{
				IsPermittedToModifySkill =
					_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.WebModifySkill),
				Skills = skillList.Select(skill => new
				{
					Id = skill.Id.Value,
					SkillType = skillTypes[skill.Id.Value],
					Workloads = skill.WorkloadCollection.Select(x => new
					{
						Id = x.Id.Value,
						Name = WorkloadNameBuilder.GetWorkloadName(skill.Name, x.Name)
					}).OrderBy(w => w.Name).ToArray()
				})
			});
		}

		[HttpPost, Route("api/Forecasting/LoadForecast"), UnitOfWork]
		public virtual IHttpActionResult LoadForecast(ForecastResultInput input)
		{
			var skill = _skillRepository.FindSkillsWithAtLeastOneQueueSource()
				.SingleOrDefault(s => s.WorkloadCollection.Any(
					w => w.Id.HasValue && w.Id.Value == input.WorkloadId));
			if (skill == null)
			{
				return Ok(new ForecastViewModel()
				{
					WorkloadId = input.WorkloadId,
					ScenarioId = input.ScenarioId,
					ForecastDays = new List<ForecastDayModel>(),
					IsSkillNotInBusinessUnit = true
				});
			}

			var period = new DateOnlyPeriod(new DateOnly(input.ForecastStart), new DateOnly(input.ForecastEnd));
			var scenario = _scenarioRepository.Get(input.ScenarioId);
			return Ok(_forecastProvider.Load(input.WorkloadId, period, scenario, input.HasUserSelectedPeriod));
		}

		[HttpPost, Route("api/Forecasting/Forecast"), ReadonlyUnitOfWork]
		public virtual IHttpActionResult Forecast(ForecastInput input)
		{
			var futurePeriod = new DateOnlyPeriod(new DateOnly(input.ForecastStart), new DateOnly(input.ForecastEnd));
			var scenario = _scenarioRepository.Get(input.ScenarioId);
			var skill = _skillRepository.FindSkillsWithAtLeastOneQueueSource()
				.SingleOrDefault(s => s.WorkloadCollection.Any(
					w => w.Id.HasValue && w.Id.Value == input.WorkloadId));

			var workload = skill.WorkloadCollection.Single(w => w.Id.Value == input.WorkloadId);
			var forecast = _forecastViewModelCreator.ForecastWorkload(workload, futurePeriod, scenario);

			return Ok(forecast);
		}

		[UnitOfWork, HttpPost, Route("api/Forecasting/QueueStatistics")]
		public virtual IHttpActionResult QueueStatistics([FromBody] Guid workloadId)
		{
			return Ok(_queueStatisticsViewModelFactory.QueueStatistics(workloadId));
		}

		[UnitOfWork, HttpPost, Route("api/Forecasting/Campaign")]
		public virtual IHttpActionResult AddCampaign(CampaignInput input)
		{
			var hasWarning = false;
			foreach (var selectedDay in input.SelectedDays)
			{
				var forecastDay = input.ForecastDays.Single(x => x.Date == selectedDay);
				if (!forecastDay.IsOpen)
				{
					continue;
				}

				if (forecastDay.HasOverride)
				{
					hasWarning = true;
					continue;
				}

				forecastDay.HasCampaign = Math.Abs(input.CampaignTasksPercent) > tolerance;
				forecastDay.IsInModification = true;
				forecastDay.CampaignTasksPercentage = input.CampaignTasksPercent;
				forecastDay.TotalTasks = (input.CampaignTasksPercent + 1) * forecastDay.Tasks;
			}

			return Ok(new
			{
				WarningMessage = hasWarning ? Resources.CampaignNotAppliedWIthExistingOverride : string.Empty,
				input.ForecastDays
			});
		}

		[UnitOfWork, HttpPost, Route("api/Forecasting/Override")]
		public virtual IHttpActionResult AddOverride(OverrideInput input)
		{
			var hasWarning = false;
			foreach (var selectedDay in input.SelectedDays)
			{
				var forecastDay = input.ForecastDays.Single(x => x.Date == selectedDay);
				if (!forecastDay.IsOpen)
				{
					continue;
				}

				forecastDay.IsInModification = true;
				if (input.ShouldOverrideTasks)
				{
					if (input.OverrideTasks.HasValue)
					{
						forecastDay.TotalTasks = input.OverrideTasks.Value;
					}
					else if (Math.Abs(forecastDay.CampaignTasksPercentage) > tolerance)
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
				if (forecastDay.HasOverride)
				{
					hasWarning = hasWarning || forecastDay.HasCampaign;
					forecastDay.HasCampaign = false;
					forecastDay.CampaignTasksPercentage = 0;
				}
				else
				{
					forecastDay.HasCampaign = Math.Abs(forecastDay.CampaignTasksPercentage) > tolerance;
				}
			}

			return Ok(new
			{
				WarningMessage = hasWarning ? Resources.ClearCampaignWIthOverride : string.Empty,
				input.ForecastDays
			});
		}

		[UnitOfWork, HttpPost, Route("api/Forecasting/ApplyForecast")]
		public virtual IHttpActionResult ApplyForecast(ForecastViewModel forecastResult)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			if (!forecastResult.ForecastDays.Any())
			{
				return Ok();
			}

			var overrideNote = $"[*{Resources.ForecastDayIsOverrided}*]";

			var workload = _workloadRepository.Get(forecastResult.WorkloadId);
			var scenario = _scenarioRepository.Get(forecastResult.ScenarioId);
			var periodStart = new DateOnly(forecastResult.ForecastDays.Min(x => x.Date).Date);
			var periodEnd = new DateOnly(forecastResult.ForecastDays.Max(x => x.Date).Date);
			var forecastPeriod = new DateOnlyPeriod(periodStart, periodEnd);
			var skillDays = _fetchAndFillSkillDays.FindRange(forecastPeriod, workload.Skill, scenario).ToDictionary(x => x.CurrentDate);
			var overrideDays = _forecastDayOverrideRepository.FindRange(forecastPeriod, workload, scenario).ToDictionary(k => k.Date);

			IEnumerable<IWorkloadDayBase> queueStatistics = new List<IWorkloadDayBase>();
			var availablePeriod = _historicalPeriodProvider.AvailablePeriod(workload);
			if (availablePeriod.HasValue)
			{
				var periodForTemplate = _historicalPeriodProvider.AvailableIntradayTemplatePeriod(availablePeriod.Value);
				queueStatistics = _loadStatistics.LoadWorkloadDay(workload, periodForTemplate);
			}

			foreach (var forecastDay in forecastResult.ForecastDays)
			{
				var skillDay = skillDays[forecastDay.Date];
				var forecastedWorkloadDay = skillDay.WorkloadDayCollection
					.SingleOrDefault(x => x.Workload.Id.Value == forecastResult.WorkloadId);
				if (!forecastedWorkloadDay.OpenForWork.IsOpen)
				{
					continue;
				}

				var model = forecastResult.ForecastDays.SingleOrDefault(x => x.Date == forecastedWorkloadDay.CurrentDate);
				forecastedWorkloadDay.Tasks = model.OverrideTasks ?? model.Tasks;
				forecastedWorkloadDay.AverageTaskTime = TimeSpan.FromSeconds(model.OverrideAverageTaskTime ?? model.AverageTaskTime);
				forecastedWorkloadDay.AverageAfterTaskTime = TimeSpan.FromSeconds(model.OverrideAverageAfterTaskTime ?? model.AverageAfterTaskTime);
				forecastedWorkloadDay.CampaignTasks = new Percent(model.CampaignTasksPercentage);

				if (model.HasOverride)
				{
					if (model.OverrideTasks.HasValue)
						forecastedWorkloadDay.CampaignTasks = new Percent(0);
					if (model.OverrideAverageTaskTime.HasValue)
						forecastedWorkloadDay.CampaignTaskTime = new Percent(0);
					if (model.OverrideAverageAfterTaskTime.HasValue)
						forecastedWorkloadDay.CampaignAfterTaskTime = new Percent(0);

					if (!overrideDays.TryGetValue(skillDay.CurrentDate, out var forecastDayOverride))
					{
						forecastedWorkloadDay.Annotation = string.IsNullOrEmpty(forecastedWorkloadDay.Annotation)
							? overrideNote
							: overrideNote + forecastedWorkloadDay.Annotation;

						forecastDayOverride = new ForecastDayOverride(skillDay.CurrentDate, workload, scenario);
						mapDayModelToOverride(forecastDayOverride, model);
						_forecastDayOverrideRepository.Add(forecastDayOverride);
					}
					else
					{
						mapDayModelToOverride(forecastDayOverride, model);
					}
				}
				else
				{
					if (overrideDays.TryGetValue(skillDay.CurrentDate, out var forecastDayOverride))
					{
						if (!string.IsNullOrEmpty(forecastedWorkloadDay.Annotation))
						{
							forecastedWorkloadDay.Annotation = forecastedWorkloadDay.Annotation.Replace(overrideNote, "");
						}

						_forecastDayOverrideRepository.Remove(forecastDayOverride);
					}
				}

				if (queueStatistics.Any())
				{
					_intradayForecaster.Apply(workload, queueStatistics, skillDay.WorkloadDayCollection);
				}
			}

			return Ok();
		}

		private static void mapDayModelToOverride(IForecastDayOverride forecastDayOverride, ForecastDayModel forecastDay)
		{
			forecastDayOverride.OriginalTasks = forecastDay.Tasks;
			forecastDayOverride.OriginalAverageTaskTime = TimeSpan.FromSeconds(forecastDay.AverageTaskTime);
			forecastDayOverride.OriginalAverageAfterTaskTime = TimeSpan.FromSeconds(forecastDay.AverageAfterTaskTime);
			forecastDayOverride.OverriddenTasks = forecastDay.OverrideTasks;
			forecastDayOverride.OverriddenAverageTaskTime = forecastDay.OverrideAverageTaskTime.HasValue
				? TimeSpan.FromSeconds(forecastDay.OverrideAverageTaskTime.Value)
				: (TimeSpan?) null;
			forecastDayOverride.OverriddenAverageAfterTaskTime = forecastDay.OverrideAverageAfterTaskTime.HasValue
				? TimeSpan.FromSeconds(forecastDay.OverrideAverageAfterTaskTime.Value)
				: (TimeSpan?) null;
		}
	}
}