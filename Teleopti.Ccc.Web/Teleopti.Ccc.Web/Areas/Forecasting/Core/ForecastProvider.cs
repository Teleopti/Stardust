using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Forecasting.Models;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public class ForecastProvider
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IFutureData _futureData;
		private readonly ForecastDayModelMapper _forecastDayModelMapper;
		private readonly IWorkloadRepository _workloadRepository;

		public ForecastProvider(
			IWorkloadRepository workloadRepository, 
			ISkillDayRepository skillDayRepository,
			IFutureData futureData,
			ForecastDayModelMapper forecastDayModelMapper)
		{
			_workloadRepository = workloadRepository;
			_skillDayRepository = skillDayRepository;
			_futureData = futureData;
			_forecastDayModelMapper = forecastDayModelMapper;
		}

		public ForecastModel Load(Guid workloadId, DateOnlyPeriod futurePeriod, IScenario scenario)
		{
			var workload = _workloadRepository.Get(workloadId);
			var skillDays = _skillDayRepository.FindRange(futurePeriod, workload.Skill, scenario);
			var futureWorkloadDays = _futureData.Fetch(workload, skillDays, futurePeriod)
				.OrderBy(x => x.CurrentDate);
			return new ForecastModel()
			{
				WorkloadId = workload.Id.Value,
				ScenarioId = scenario.Id.Value,
				ForecastDays = createModelDays(futureWorkloadDays)
			};
		}

		private IList<ForecastDayModel> createModelDays(IEnumerable<IWorkloadDayBase> workloadDays)
		{
			var days = new List<ForecastDayModel>();
			foreach (var workloadDay in workloadDays)
			{
				var dayModel = new ForecastDayModel
				{
					Date = workloadDay.CurrentDate,
					Tasks = workloadDay.Tasks,
					TotalTasks = workloadDay.TotalTasks,
					AverageTaskTime = workloadDay.AverageTaskTime.TotalSeconds,
					TotalAverageTaskTime = workloadDay.TotalAverageTaskTime.TotalSeconds,
					AverageAfterTaskTime = workloadDay.AverageAfterTaskTime.TotalSeconds,
					TotalAverageAfterTaskTime = workloadDay.TotalAverageAfterTaskTime.TotalSeconds,
					IsOpen = workloadDay.OpenForWork.IsOpen
				};

				_forecastDayModelMapper.SetCampaignAndOverride(workloadDay, dayModel);

				days.Add(dayModel);
			}

			return days.ToArray();
		}
	}
}