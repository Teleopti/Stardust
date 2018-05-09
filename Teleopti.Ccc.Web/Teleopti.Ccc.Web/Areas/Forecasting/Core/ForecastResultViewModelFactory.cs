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
	public class ForecastResultViewModelFactory : IForecastResultViewModelFactory
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IFutureData _futureData;
		private readonly IWorkloadRepository _workloadRepository;

		public ForecastResultViewModelFactory(IWorkloadRepository workloadRepository, ISkillDayRepository skillDayRepository,
			IFutureData futureData)
		{
			_workloadRepository = workloadRepository;
			_skillDayRepository = skillDayRepository;
			_futureData = futureData;
		}

		public ForecastModel Create(Guid workloadId, DateOnlyPeriod futurePeriod, IScenario scenario)
		{
			var workload = _workloadRepository.Get(workloadId);
			var skillDays = _skillDayRepository.FindRange(futurePeriod, workload.Skill, scenario);
			var futureWorkloadDays = _futureData.Fetch(workload, skillDays, futurePeriod)
				.OrderBy(x => x.CurrentDate);
			return new ForecastModel()
			{
				WorkloadId = workload.Id.Value,
				ScenarioId = scenario.Id.Value,
				ForecastDays = transformForecastResult(futureWorkloadDays)
			};
		}

		private IList<ForecastDayModel> transformForecastResult(IEnumerable<IWorkloadDayBase> workloadDays)
		{
			var days = new List<ForecastDayModel>();
			foreach (var workloadDay in workloadDays)
			{
				var day = new ForecastDayModel
				{
					Date = workloadDay.CurrentDate,
					Tasks = workloadDay.Tasks,
					TotalTasks = workloadDay.TotalTasks,
					AverageTaskTime = workloadDay.AverageTaskTime.TotalSeconds,
					TotalAverageTaskTime = workloadDay.TotalAverageTaskTime.TotalSeconds,
					AverageAfterTaskTime = workloadDay.AverageAfterTaskTime.TotalSeconds,
					TotalAverageAfterTaskTime = workloadDay.TotalAverageAfterTaskTime.TotalSeconds
				};

				var hasOverride = workloadDay.OverrideTasks.HasValue ||
								  workloadDay.OverrideAverageTaskTime.HasValue ||
								  workloadDay.OverrideAverageAfterTaskTime.HasValue;
				var hasCampaign = Math.Abs(workloadDay.CampaignTasks.Value) > 0;
				if (hasCampaign && hasOverride)
				{
					day.HasCampaign = true;
					day.HasOverride = true;
				}
				else if (hasOverride)
				{
					day.HasOverride = true;
				}
				else if (hasCampaign)
				{
					day.HasCampaign = true;
				}

				days.Add(day);
			}

			return days.ToArray();
		}
	}
}