using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze
{
	public interface IIntraIntervalFinder
	{
		IEnumerable<DateTimePeriod> FindForInterval(DateTimePeriod interval, IResourceCalculationDataContainer resourceCalculationDataContainer, ISkill skill);
	}

	public class IntraIntervalFinder : IIntraIntervalFinder
	{
		public IEnumerable<DateTimePeriod> FindForInterval(DateTimePeriod interval, IResourceCalculationDataContainer resourceCalculationDataContainer, ISkill skill)
		{
			var relevantProjections = resourceCalculationDataContainer as IResourceCalculationDataContainerWithSingleOperation;
			var resultList = relevantProjections.IntraIntervalResources(skill, interval);
			return resultList;
		}
	}
}
