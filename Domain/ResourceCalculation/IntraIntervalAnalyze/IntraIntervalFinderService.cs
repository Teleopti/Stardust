﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze
{
	public interface IIntraIntervalFinderService
	{
		void Execute(IEnumerable<ISkillDay> allSkillDays, DateOnly dateOnly, IResourceCalculationDataContainer resourceCalculationDataContainer);
	}

	public class IntraIntervalFinderService : IIntraIntervalFinderService
	{
		private readonly ISkillDayIntraIntervalFinder _skillDayIntraIntervalFinder;

		public IntraIntervalFinderService(ISkillDayIntraIntervalFinder skillDayIntraIntervalFinder)
		{
			_skillDayIntraIntervalFinder = skillDayIntraIntervalFinder;
		}

		public void Execute(IEnumerable<ISkillDay> allSkillDays, DateOnly dateOnly, IResourceCalculationDataContainer resourceCalculationDataContainer)
		{
			var skillDays = allSkillDays.FilterOnDates(new []{dateOnly});
			
			foreach (var skillDay in skillDays)
			{
				var forcastSource = skillDay.Skill.SkillType.ForecastSource;
				if (forcastSource != ForecastSource.InboundTelephony && forcastSource != ForecastSource.Chat)
					continue;

				_skillDayIntraIntervalFinder.SetIntraIntervalIssues(skillDay, resourceCalculationDataContainer, 0.7999);
			}
		}
	}
}
