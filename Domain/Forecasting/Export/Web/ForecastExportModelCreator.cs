using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Forecasting.Export.Web
{
	public class ForecastExportModelCreator
	{
		private readonly SkillDayLoadHelper _skillDayLoadHelper;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IWorkloadRepository _workloadRepository;

		public ForecastExportModelCreator(
			SkillDayLoadHelper skillDayLoadHelper,
			IScenarioRepository scenarioRepository,
			IWorkloadRepository workloadRepository)
		{
			_skillDayLoadHelper = skillDayLoadHelper;
			_scenarioRepository = scenarioRepository;
			_workloadRepository = workloadRepository;
		}
		public ForecastExportModel Load(Guid scenarioId, Guid workloadId, DateOnlyPeriod period)
		{
			var scenario = _scenarioRepository.Get(scenarioId);
			var workload = _workloadRepository.Get(workloadId);
			var skill = workload.Skill;
			var skillDaysBySkills = _skillDayLoadHelper.LoadSchedulerSkillDays(period, new List<ISkill> { workload.Skill }, scenario);

			var skillDays = skillDaysBySkills[workload.Skill]
				.Where(skillDay => skillDay.CurrentDate >= period.StartDate && skillDay.CurrentDate <= period.EndDate)
				.ToList();
			return new ForecastExportModel
			{
				Header = ForecastExportHeaderModelCreator.Load(scenario.Description.Name, workload, skillDays, period),
				DailyModelForecast = ForecastExportDailyModelCreator.Load(workloadId, skillDays),
				IntervalModelForecast = ForecastExportIntervalModelCreator.Load(workload, skillDays),
				Workloads = skill.WorkloadCollection.Select(w=>WorkloadNameBuilder.GetWorkloadName(skill.Name, w.Name)).ToList()
			};
		}
	}
}