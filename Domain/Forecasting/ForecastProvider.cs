using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public class ForecastProvider
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IFutureData _futureData;
		private readonly ForecastDayModelMapper _forecastDayModelMapper;
		private readonly IForecastDayOverrideRepository _forecastDayOverrideRepository;
		private readonly IWorkloadRepository _workloadRepository;

		public ForecastProvider(
			IWorkloadRepository workloadRepository, 
			ISkillDayRepository skillDayRepository,
			IFutureData futureData,
			ForecastDayModelMapper forecastDayModelMapper,
			IForecastDayOverrideRepository forecastDayOverrideRepository)
		{
			_workloadRepository = workloadRepository;
			_skillDayRepository = skillDayRepository;
			_futureData = futureData;
			_forecastDayModelMapper = forecastDayModelMapper;
			_forecastDayOverrideRepository = forecastDayOverrideRepository;
		}

		public ForecastViewModel Load(Guid workloadId, DateOnlyPeriod futurePeriod, IScenario scenario, bool hasUserSelectedPeriod)
		{
			var workload = _workloadRepository.Get(workloadId);
			var skillDays = _skillDayRepository.FindRange(futurePeriod, workload.Skill, scenario);
			var futureWorkloadDays = _futureData.Fetch(workload, skillDays, futurePeriod)
				.OrderBy(x => x.CurrentDate)
				.ToList();
			var overrideDays = _forecastDayOverrideRepository.FindRange(futurePeriod, workload, scenario)
				.ToDictionary(x => x.Date);
			return new ForecastViewModel()
			{
				WorkloadId = workload.Id.Value,
				ScenarioId = scenario.Id.Value,
				ForecastDays = createModelDays(futureWorkloadDays, overrideDays, futurePeriod, hasUserSelectedPeriod)
			};
		}

		private IList<ForecastDayModel> createModelDays(IList<IWorkloadDayBase> workloadDays,
			Dictionary<DateOnly, IForecastDayOverride> overrideDays, 
			DateOnlyPeriod futurePeriod,
			bool hasUserSelectedPeriod)
		{
			var days = new List<ForecastDayModel>();

			if (!workloadDays.Any())
			{
				return days;
			}

			DateOnly dayModelStart;
			DateOnly dayModelEnd;
			if (hasUserSelectedPeriod)
			{
				dayModelStart = futurePeriod.StartDate;
				dayModelEnd = futurePeriod.EndDate;
			}
			else
			{
				dayModelStart = workloadDays.Min(x => x.CurrentDate);
				dayModelEnd = workloadDays.Max(x => x.CurrentDate);
			}

			for (var date = dayModelStart; date <= dayModelEnd; date = date.AddDays(1))
			{
				var workloadDay = workloadDays.SingleOrDefault(x => x.CurrentDate == date);
				ForecastDayModel dayModel;
				if (workloadDay == null)
				{
					dayModel= new ForecastDayModel()
					{
						Date = date,
						IsForecasted = false
					};
				}
				else
				{
					overrideDays.TryGetValue(workloadDay.CurrentDate, out var overrideDay);
					dayModel = new ForecastDayModel
					{
						Date = workloadDay.CurrentDate,
						Tasks = overrideDay?.OriginalTasks ?? workloadDay.Tasks,
						TotalTasks = workloadDay.TotalTasks,
						AverageTaskTime = overrideDay?.OriginalAverageTaskTime.TotalSeconds ?? workloadDay.AverageTaskTime.TotalSeconds,
						TotalAverageTaskTime = workloadDay.TotalAverageTaskTime.TotalSeconds,
						AverageAfterTaskTime = overrideDay?.OriginalAverageAfterTaskTime.TotalSeconds ?? workloadDay.AverageAfterTaskTime.TotalSeconds,
						TotalAverageAfterTaskTime = workloadDay.TotalAverageAfterTaskTime.TotalSeconds,
						IsOpen = workloadDay.OpenForWork.IsOpen,
						IsForecasted = true
					};

					_forecastDayModelMapper.SetCampaignAndOverride(workloadDay, dayModel, overrideDay);
				}
				days.Add(dayModel);
			}

			return days.ToArray();
		}
	}
}