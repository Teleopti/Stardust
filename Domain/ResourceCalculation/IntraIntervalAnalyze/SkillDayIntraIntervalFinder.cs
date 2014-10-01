using Teleopti.Ccc.Domain.Collection;
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
		private readonly ISkillActivityCountCollector _countCollector;
		private readonly IFullIntervalFinder _fullIntervalFinder;

		public SkillDayIntraIntervalFinder(IIntraIntervalFinder intraIntervalFinder, ISkillActivityCountCollector countCollector, IFullIntervalFinder fullIntervalFinder)
		{
			_intraIntervalFinder = intraIntervalFinder;
			_countCollector = countCollector;
			_fullIntervalFinder = fullIntervalFinder;
		}

		public void SetIntraIntervalIssues(ISkillDay skillDay, IResourceCalculationDataContainer resourceCalculationDataContainer)
		{
			var skill = skillDay.Skill;
			foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
			{
				var list = _intraIntervalFinder.FindForInterval(skillStaffPeriod.Period, resourceCalculationDataContainer, skill);

				if(list.IsEmpty()) continue;

				var result = _countCollector.Collect(list, skillStaffPeriod.Period);
				var fullInterval = _fullIntervalFinder.FindForInterval(skillStaffPeriod.Period, resourceCalculationDataContainer, skill, list);

				var min = double.MaxValue;
				var max = double.MinValue;

				foreach (var i in result)
				{
					if ((i + fullInterval) < min) min = i + fullInterval;
					if ((i + fullInterval) > max) max = i + fullInterval;
				}

				skillStaffPeriod.HasIntraIntervalIssue = min / max < 0.5;		
			}
		}
	}
}
