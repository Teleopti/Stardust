using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze
{
	public interface IIntraIntervalFinder
	{
		bool FindForInterval(DateTimePeriod interval, IResourceCalculationDataContainer resourceCalculationDataContainer, ISkill skill);
	}

	public class IntraIntervalFinder : IIntraIntervalFinder
	{
		public bool FindForInterval(DateTimePeriod interval, IResourceCalculationDataContainer resourceCalculationDataContainer, ISkill skill)
		{
			var relevantProjections = resourceCalculationDataContainer as IResourceCalculationDataContainerWithSingleOperation;
			var resultList = relevantProjections.IntraIntervalResources(skill, interval);
			return resultList.Any();
		}
	}
}
