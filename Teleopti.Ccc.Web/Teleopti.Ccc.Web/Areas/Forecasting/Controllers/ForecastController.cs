using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Angel;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
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
		private readonly ForecastProvider _forecastProvider;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IWorkloadRepository _workloadRepository;
		private readonly IAuthorization _authorization;
		private readonly IWorkloadNameBuilder _workloadNameBuilder;
		private readonly IFetchAndFillSkillDays _fetchAndFillSkillDays;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;
		private readonly IIntradayForecaster _intradayForecaster;
		private readonly IForecastDayOverrideRepository _forecastDayOverrideRepository;

		public ForecastController(
			IForecastCreator forecastCreator,
			ISkillRepository skillRepository,
			ForecastProvider forecastProvider,
			IScenarioRepository scenarioRepository,
			IWorkloadRepository workloadRepository,
			IAuthorization authorization,
			IWorkloadNameBuilder workloadNameBuilder,
			IFetchAndFillSkillDays fetchAndFillSkillDays,
			IHistoricalPeriodProvider historicalPeriodProvider,
			IIntradayForecaster intradayForecaster,
			IForecastDayOverrideRepository forecastDayOverrideRepository)
		{
			_forecastCreator = forecastCreator;
			_skillRepository = skillRepository;
			_forecastProvider = forecastProvider;
			_scenarioRepository = scenarioRepository;
			_workloadRepository = workloadRepository;
			_authorization = authorization;
			_workloadNameBuilder = workloadNameBuilder;
			_fetchAndFillSkillDays = fetchAndFillSkillDays;
			_historicalPeriodProvider = historicalPeriodProvider;
			_intradayForecaster = intradayForecaster;
			_forecastDayOverrideRepository = forecastDayOverrideRepository;
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

		[HttpPost, Route("api/Forecasting/LoadForecast"), UnitOfWork]
		public virtual IHttpActionResult LoadForecast(ForecastResultInput input)
		{
			var period = new DateOnlyPeriod(new DateOnly(input.ForecastStart), new DateOnly(input.ForecastEnd));
			var scenario = _scenarioRepository.Get(input.ScenarioId);
			return Ok(_forecastProvider.Load(input.WorkloadId, period, scenario));
		}

		[HttpPost, Route("api/Forecasting/Forecast"), ReadonlyUnitOfWork]
		public virtual IHttpActionResult Forecast(ForecastInput input)
		{
			var futurePeriod = new DateOnlyPeriod(new DateOnly(input.ForecastStart), new DateOnly(input.ForecastEnd));
			var scenario = _scenarioRepository.Get(input.ScenarioId);
			var forecast = _forecastCreator.CreateForecastForWorkload(futurePeriod, input.Workload, scenario);
			return Ok(forecast);
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

				forecastDay.HasCampaign = input.CampaignTasksPercent > 0d;
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
				if (forecastDay.HasOverride)
				{
					hasWarning = hasWarning || forecastDay.HasCampaign;
					forecastDay.HasCampaign = false;
					forecastDay.CampaignTasksPercentage = 0;
				}
				else
				{
					forecastDay.HasCampaign = forecastDay.CampaignTasksPercentage > 0;
				}
			}

			return Ok(new
			{
				WarningMessage = hasWarning ? Resources.ClearCampaignWIthOverride : string.Empty,
				input.ForecastDays
			});
		}

		[UnitOfWork, HttpPost, Route("api/Forecasting/ApplyForecast")]
		public virtual IHttpActionResult ApplyForecast(ForecastModel forecastResult)
		{
			var overrideNote = $"[*{Resources.ForecastDayIsOverrided}*]";

			var workload = _workloadRepository.Get(forecastResult.WorkloadId);
			var scenario = _scenarioRepository.Get(forecastResult.ScenarioId);
			var periodStart = new DateOnly(forecastResult.ForecastDays.Min(x => x.Date).Date);
			var periodEnd = new DateOnly(forecastResult.ForecastDays.Max(x => x.Date).Date);
			var forecastPeriod = new DateOnlyPeriod(periodStart, periodEnd);
			var skillDays = _fetchAndFillSkillDays.FindRange(forecastPeriod, workload.Skill, scenario);
			var overrideDays = _forecastDayOverrideRepository.FindRange(forecastPeriod, workload, scenario).ToDictionary(k => k.Date);

			foreach (var skillDay in skillDays)
			{
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
				
				var period = _historicalPeriodProvider.AvailablePeriod(workload);
				if (period.HasValue)
				{
					var periodForTemplate = _historicalPeriodProvider.AvailableIntradayTemplatePeriod(period.Value);
					_intradayForecaster.Apply(workload, periodForTemplate, skillDay.WorkloadDayCollection);
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