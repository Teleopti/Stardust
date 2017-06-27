using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export.Web
{
	public class ForecastExportModelCreator
	{
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IWorkloadRepository _workloadRepository;

		public ForecastExportModelCreator(
			ISkillDayLoadHelper skillDayLoadHelper,
			IScenarioRepository scenarioRepository,
			IWorkloadRepository workloadRepository)
		{
			_skillDayLoadHelper = skillDayLoadHelper;
			_scenarioRepository = scenarioRepository;
			_workloadRepository = workloadRepository;
		}
		public ForecastExportModel Load(Guid workloadId, DateOnlyPeriod period)
		{
			var scenario = _scenarioRepository.LoadDefaultScenario();
			var workload = _workloadRepository.Get(workloadId);
			var skillDaysBySkills = _skillDayLoadHelper.LoadSchedulerSkillDays(period, new List<ISkill> { workload.Skill }, scenario);

			var skillDays = skillDaysBySkills[workload.Skill]
				.Where(skillDay => skillDay.CurrentDate >= period.StartDate && skillDay.CurrentDate <= period.EndDate)
				.ToList();
			return new ForecastExportModel
			{
				Header = ForecastExportHeaderModelCreator.Load(workload.Skill, skillDays, period),
				DailyModelForecast = ForecastExportDailyModelCreator.Load(skillDays),
				IntervalModelForecast = ForecastExportIntervalModelCreator.Load(workload, skillDays)
			};
		}
	}
}