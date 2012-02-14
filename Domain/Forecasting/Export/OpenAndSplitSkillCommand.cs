using System.Linq;
using System.Reflection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
	public class OpenAndSplitSkillCommand : IOpenAndSplitSkillCommand
	{
		private readonly IScenarioProvider _scenarioProvider;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly WorkloadDayHelper _workloadDayHelper;

		public OpenAndSplitSkillCommand(IScenarioProvider scenarioProvider, ISkillDayRepository skillDayRepository, WorkloadDayHelper workloadDayHelper)
		{
			_scenarioProvider = scenarioProvider;
			_workloadDayHelper = workloadDayHelper;
			_skillDayRepository = skillDayRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Execute(ISkill skill, DateOnlyPeriod period)
		{
			var scenario = _scenarioProvider.DefaultScenario(skill.BusinessUnit);
			var skillDays = _skillDayRepository.FindRange(period, skill, scenario);
			var allSkillDays = _skillDayRepository.GetAllSkillDays(period, skillDays, skill, scenario, true);

			var workloadDays = _workloadDayHelper.GetWorkloadDaysFromSkillDays(allSkillDays, skill.WorkloadCollection.First());
			foreach (var workloadDayBase in workloadDays)
			{
				workloadDayBase.MakeOpen24Hours();
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
			typeof(AggregateRootWithBusinessUnit).GetField("_businessUnit", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(
				skillDay, businessUnit);
		}
	}
}