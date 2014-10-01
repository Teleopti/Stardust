using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze
{
	public interface IFullIntervalFinder
	{
		double FindForInterval(DateTimePeriod interval, IResourceCalculationDataContainer resourceCalculationDataContainer, ISkill skill, IEnumerable<DateTimePeriod> intraIntervalPeriods);
	}

	public class FullIntervalFinder : IFullIntervalFinder
	{
		public double FindForInterval(DateTimePeriod interval, IResourceCalculationDataContainer resourceCalculationDataContainer, ISkill skill, IEnumerable<DateTimePeriod> intraIntervalPeriods)
		{
			var intraIntervalMinutes = intraIntervalPeriods.Sum(intraIntervalPeriod => intraIntervalPeriod.ElapsedTime().Minutes);
			var fullIntraIntervalHeads = (double)intraIntervalMinutes / interval.ElapsedTime().Minutes;
			var fullIntervalHeads = resourceCalculationDataContainer.SkillResources(skill, interval);

			return fullIntervalHeads.Item2 - fullIntraIntervalHeads;
		}
	}
}
