using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public class ForecastResultViewModelFactory : IForecastResultViewModelFactory
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IFutureData _futureData;
		private readonly IWorkloadRepository _workloadRepository;

		public ForecastResultViewModelFactory(IWorkloadRepository workloadRepository, ISkillDayRepository skillDayRepository, IFutureData futureData)
		{
			_workloadRepository = workloadRepository;
			_skillDayRepository = skillDayRepository;
			_futureData = futureData;
		}

		public WorkloadForecastResultViewModel Create(Guid workloadId, DateOnlyPeriod futurePeriod, IScenario scenario)
		{
			var workload = _workloadRepository.Get(workloadId);
			var skillDays = _skillDayRepository.FindRange(futurePeriod, workload.Skill, scenario);
			var futureWorkloadDays = _futureData.Fetch(workload, skillDays, futurePeriod);
			return new WorkloadForecastResultViewModel
			{
				WorkloadId = workload.Id.GetValueOrDefault(),
				Days = transformForecastResult(futureWorkloadDays)
			};
		}

		private dynamic[] transformForecastResult(IEnumerable<IWorkloadDayBase> workloadDays)
		{
			var days = new List<dynamic>();
			foreach (var workloadDay in workloadDays)
			{
				dynamic day = new ExpandoObject();
				day.date = workloadDay.CurrentDate.Date;
				day.vc = workloadDay.Tasks;
				day.vtc = workloadDay.TotalTasks;
				day.vtt = workloadDay.AverageTaskTime.TotalSeconds;
				day.vttt = workloadDay.TotalAverageTaskTime.TotalSeconds;
				day.vacw = workloadDay.AverageAfterTaskTime.TotalSeconds;
				day.vtacw = workloadDay.TotalAverageAfterTaskTime.TotalSeconds;

				if (Math.Abs(workloadDay.CampaignTasks.Value) > 0
					&& !workloadDay.OverrideTasks.HasValue 
					&& (workloadDay.OverrideAverageTaskTime.HasValue || workloadDay.OverrideAverageAfterTaskTime.HasValue))
				{
					day.vcombo = 1;
				}
				else if (workloadDay.OverrideTasks.HasValue
							|| workloadDay.OverrideAverageTaskTime.HasValue
							|| workloadDay.OverrideAverageAfterTaskTime.HasValue)
				{
					day.voverride = 1;
				}
				else if (Math.Abs(workloadDay.CampaignTasks.Value) > 0)
				{
					day.vcampaign = 1;
				}

				days.Add(day);
			}

			return days.ToArray();
		}
	}
}