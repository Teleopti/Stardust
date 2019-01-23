using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze
{
	public interface IIntraIntervalFinderService
	{
		void Execute(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, DateOnly dateOnly, IResourceCalculationDataContainer resourceCalculationDataContainer);
	}

	public class IntraIntervalFinderService : IIntraIntervalFinderService
	{
		private readonly ISkillDayIntraIntervalFinder _skillDayIntraIntervalFinder;

		public IntraIntervalFinderService(ISkillDayIntraIntervalFinder skillDayIntraIntervalFinder)
		{
			_skillDayIntraIntervalFinder = skillDayIntraIntervalFinder;
		}

		public void Execute(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, DateOnly dateOnly, IResourceCalculationDataContainer resourceCalculationDataContainer)
		{
			foreach (var skillDay in skillDays.FilterOnDates(new HashSet<DateOnly>{ dateOnly }))
			{
				var forcastSource = skillDay.Skill.SkillType.ForecastSource;
				if (forcastSource != ForecastSource.InboundTelephony && forcastSource != ForecastSource.Chat)
					continue;

				_skillDayIntraIntervalFinder.SetIntraIntervalIssues(skillDay, resourceCalculationDataContainer, 0.7999);
			}
		}
	}
}
