using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

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

		public ForecastModel Load(Guid workloadId, DateOnlyPeriod futurePeriod, IScenario scenario)
		{
			var workload = _workloadRepository.Get(workloadId);
			var skillDays = _skillDayRepository.FindRange(futurePeriod, workload.Skill, scenario);
			var futureWorkloadDays = _futureData.Fetch(workload, skillDays, futurePeriod)
				.OrderBy(x => x.CurrentDate);
			var overrideDays = _forecastDayOverrideRepository.FindRange(futurePeriod, workload, scenario)
				.ToDictionary(x => x.Date);
			return new ForecastModel()
			{
				WorkloadId = workload.Id.Value,
				ScenarioId = scenario.Id.Value,
				ForecastDays = createModelDays(futureWorkloadDays, overrideDays)
			};
		}

		private IList<ForecastDayModel> createModelDays(IEnumerable<IWorkloadDayBase> workloadDays,
			Dictionary<DateOnly, IForecastDayOverride> overrideDays)
		{
			var days = new List<ForecastDayModel>();
			foreach (var workloadDay in workloadDays)
			{
				overrideDays.TryGetValue(workloadDay.CurrentDate, out var overrideDay);
				var dayModel = new ForecastDayModel
				{
					Date = workloadDay.CurrentDate,
					Tasks = overrideDay?.OriginalTasks ?? workloadDay.Tasks,
					TotalTasks = workloadDay.TotalTasks,
					AverageTaskTime = overrideDay?.OriginalAverageTaskTime.TotalSeconds ?? workloadDay.AverageTaskTime.TotalSeconds,
					TotalAverageTaskTime = workloadDay.TotalAverageTaskTime.TotalSeconds,
					AverageAfterTaskTime = overrideDay?.OriginalAverageAfterTaskTime.TotalSeconds ?? workloadDay.AverageAfterTaskTime.TotalSeconds,
					TotalAverageAfterTaskTime = workloadDay.TotalAverageAfterTaskTime.TotalSeconds,
					IsOpen = workloadDay.OpenForWork.IsOpen
				};

				_forecastDayModelMapper.SetCampaignAndOverride(workloadDay, dayModel, overrideDay);

				days.Add(dayModel);
			}

			return days.ToArray();
		}
	}
}