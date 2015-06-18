using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public class ForecastResultViewModelFactory : IForecastResultViewModelFactory
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly ICurrentScenario _currentScenario;
		private readonly IFutureData _futureData;
		private readonly IWorkloadRepository _workloadRepository;

		public ForecastResultViewModelFactory(IWorkloadRepository workloadRepository, ISkillDayRepository skillDayRepository, ICurrentScenario currentScenario, IFutureData futureData)
		{
			_workloadRepository = workloadRepository;
			_skillDayRepository = skillDayRepository;
			_currentScenario = currentScenario;
			_futureData = futureData;
		}

		public WorkloadForecastResultViewModel Create(Guid workloadId, DateOnlyPeriod futurePeriod)
		{
			var workload = _workloadRepository.Get(workloadId);
			var currentScenario = _currentScenario.Current();
			var skillDays = _skillDayRepository.FindRange(futurePeriod, workload.Skill, currentScenario);
			var futureWorkloadDays = _futureData.Fetch(workload, skillDays, futurePeriod);
			return new WorkloadForecastResultViewModel
			{
				WorkloadId = workloadId,
				Days = futureWorkloadDays.Select(x => new { date = x.CurrentDate.Date, vc = Math.Round((double) x.Tasks, 1), vaht = Math.Round((double) x.AverageTaskTime.TotalSeconds, 1), vacw = Math.Round((double) x.AverageAfterTaskTime.TotalSeconds, 1) }).ToArray<dynamic>()
			};
		}
	}
}