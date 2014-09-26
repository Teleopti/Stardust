using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze
{
	public interface IIntraIntervalFinderService
	{
		void Execute(ISchedulingResultStateHolder schedulingResultStateHolder, DateOnly dateOnly, IResourceCalculationDataContainer resourceCalculationDataContainer);
	}

	public class IntraIntervalFinderService : IIntraIntervalFinderService
	{
		private readonly ISkillDayIntraIntervalFinder _skillDayIntraIntervalFinder;

		public IntraIntervalFinderService(ISkillDayIntraIntervalFinder skillDayIntraIntervalFinder)
		{
			_skillDayIntraIntervalFinder = skillDayIntraIntervalFinder;
		}

		public void Execute(ISchedulingResultStateHolder schedulingResultStateHolder, DateOnly dateOnly, IResourceCalculationDataContainer resourceCalculationDataContainer)
		{
			var skillDays = schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly>{dateOnly});
			
			foreach (var skillDay in skillDays)
			{
				if(skillDay.Skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill)
					continue;

				_skillDayIntraIntervalFinder.SetIntraIntervalIssues(skillDay, resourceCalculationDataContainer);
			}
		}
	}

	public class IntraIntervalFinderServiceToggle29845Off : IIntraIntervalFinderService
	{
		public void Execute(ISchedulingResultStateHolder schedulingResultStateHolder, DateOnly dateOnly, IResourceCalculationDataContainer resourceCalculationDataContainer)
		{
			
		}
	}
}
