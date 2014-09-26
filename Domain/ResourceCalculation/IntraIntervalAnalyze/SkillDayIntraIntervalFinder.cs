
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze
{
	public interface ISkillDayIntraIntervalFinder
	{
		void SetIntraIntervalIssues(ISkillDay skillDay, IResourceCalculationDataContainer resourceCalculationDataContainer	);
	}

	public class SkillDayIntraIntervalFinder : ISkillDayIntraIntervalFinder
	{
		private readonly IIntraIntervalFinder _intraIntervalFinder;

		public SkillDayIntraIntervalFinder(IIntraIntervalFinder intraIntervalFinder)
		{
			_intraIntervalFinder = intraIntervalFinder;
		}

		public void SetIntraIntervalIssues(ISkillDay skillDay, IResourceCalculationDataContainer resourceCalculationDataContainer)
		{
			var skill = skillDay.Skill;
			foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
			{
				skillStaffPeriod.HasIntraIntervalIssue = _intraIntervalFinder.FindForInterval(skillStaffPeriod.Period, resourceCalculationDataContainer, skill);
			}
		}
	}
}
