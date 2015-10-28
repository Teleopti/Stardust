using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public class OverrideTasksPersister : IOverrideTasksPersister
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IFutureData _futureData;

		public OverrideTasksPersister(ISkillDayRepository skillDayRepository, IFutureData futureData)
		{
			_skillDayRepository = skillDayRepository;
			_futureData = futureData;
		}

		public void Persist(IScenario scenario, IWorkload workload, ModifiedDay[] days, int overrideTasks)
		{
			var min = days.Min(x => x.Date);
			var max = days.Max(x => x.Date);
			var futurePeriod = new DateOnlyPeriod(new DateOnly(min), new DateOnly(max));
			var skillDays = _skillDayRepository.FindRange(futurePeriod, workload.Skill, scenario);
			var workloadDays = _futureData.Fetch(workload, skillDays, futurePeriod);

			foreach (var workloadDay in days.Select(day => workloadDays.Single(x => x.CurrentDate == new DateOnly(day.Date))).Where(workloadDay => workloadDay.OpenForWork.IsOpen))
			{
				workloadDay.OverrideTasks = overrideTasks;
			}
		}
	}
}