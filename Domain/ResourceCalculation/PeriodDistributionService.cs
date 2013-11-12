using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class PeriodDistributionService : IPeriodDistributionService
    {
	    private const int _intraIntervalLengthToSplitOn = 5;

	    public void CalculateDay(IResourceCalculationDataContainer resourceContainer, ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriods)
        {
	        var relevantProjections = resourceContainer as IResourceCalculationDataContainerWithSingleOperation;
			if (relevantProjections==null) return;

            foreach (var skillStaffPeriodDictionary in skillStaffPeriods)
            {
                foreach (var pair in skillStaffPeriodDictionary.Value)
                {
                    var period = pair.Key;
                    var demandedTraff = pair.Value.FStaff;
                    var periodDistribution =
                        new PeriodDistribution(pair.Value, skillStaffPeriodDictionary.Key.Activity, period, _intraIntervalLengthToSplitOn, demandedTraff);

                    pair.Value.ClearIntraIntervalDistribution();

					periodDistribution.ProcessLayers(relevantProjections);
                }
            }
        }
    }
 }
