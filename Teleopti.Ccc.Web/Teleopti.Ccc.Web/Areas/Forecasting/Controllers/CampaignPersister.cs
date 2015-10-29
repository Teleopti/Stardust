using System;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Future;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Controllers
{
	public class CampaignPersister : ICampaignPersister
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly IFutureData _futureData;

		public CampaignPersister(ISkillDayRepository skillDayRepository, IFutureData futureData)
		{
			_skillDayRepository = skillDayRepository;
			_futureData = futureData;
		}

		public void Persist(IScenario scenario, IWorkload workload, ModifiedDay[] days, double campaignTasksPercent)
		{
			var min = days.Min(x => x.Date);
			var max = days.Max(x => x.Date);
			var futurePeriod = new DateOnlyPeriod(new DateOnly(min), new DateOnly(max));
			var skillDays = _skillDayRepository.FindRange(futurePeriod, workload.Skill, scenario);
			var workloadDays = _futureData.Fetch(workload, skillDays, futurePeriod);

			foreach (var workloadDay in days.Select(campaignDay => workloadDays.Single(x => x.CurrentDate == new DateOnly(campaignDay.Date))).Where(workloadDay => workloadDay.OpenForWork.IsOpen))
			{
				workloadDay.CampaignTasks = new Percent(campaignTasksPercent / 100d);
			}
		}
	}
}