using System;
using System.Linq;
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
				Days = futureWorkloadDays.Select(x => new
				{
					date = x.CurrentDate.Date,
					vc = x.Tasks,
					vtc = x.TotalTasks,
					vaht = x.AverageTaskTime.TotalSeconds,
					vacw = x.AverageAfterTaskTime.TotalSeconds
				}).ToArray<dynamic>()
			};
		}
	}
}