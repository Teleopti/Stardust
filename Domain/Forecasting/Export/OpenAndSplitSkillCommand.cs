using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
	public class OpenAndSplitSkillCommand : IOpenAndSplitSkillCommand
	{
		private readonly IScenarioRepository _scenarioRepository;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly WorkloadDayHelper _workloadDayHelper;

		public OpenAndSplitSkillCommand(IScenarioRepository scenarioRepository, ISkillDayRepository skillDayRepository, WorkloadDayHelper workloadDayHelper)
		{
			_scenarioRepository = scenarioRepository;
			_workloadDayHelper = workloadDayHelper;
			_skillDayRepository = skillDayRepository;
		}

	    public void Execute(ISkill skill, DateOnlyPeriod period, IList<TimePeriod> openHoursList)
	    {
            var scenario = _scenarioRepository.LoadDefaultScenario(skill.BusinessUnit);
            var skillDays = _skillDayRepository.FindRange(period, skill, scenario);
			var allSkillDays = _skillDayRepository.GetAllSkillDays(period, skillDays, skill, scenario, s =>
				{ 
					s.ForEach(sd => sd.SplitSkillDataPeriods(sd.SkillDataPeriodCollection.ToList()));
					_skillDayRepository.AddRange(s);
				});

            var workloadDays = _workloadDayHelper.GetWorkloadDaysFromSkillDays(allSkillDays, skill.WorkloadCollection.First());
            foreach (var workloadDayBase in workloadDays)
            {
                workloadDayBase.ChangeOpenHours(openHoursList);
                workloadDayBase.SplitTemplateTaskPeriods(workloadDayBase.TaskPeriodList.ToList());
                var skillDay = workloadDayBase.Parent as ISkillDay;
                if (skillDay != null)
				{
					skillDay.SplitSkillDataPeriods(skillDay.SkillDataPeriodCollection.ToList());
                    setTargetBusinessUnit(skillDay, skill.BusinessUnit);
                }
            }
	    }

	    private static void setTargetBusinessUnit(ISkillDay skillDay, IBusinessUnit businessUnit)
		{
			typeof(VersionedAggregateRootWithBusinessUnit).GetField("_businessUnit", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).SetValue(
				skillDay, businessUnit);
		}
	}
}