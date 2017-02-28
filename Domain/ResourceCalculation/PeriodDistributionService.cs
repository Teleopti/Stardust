using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    public class PeriodDistributionService : IPeriodDistributionService
    {
	    private const int _intraIntervalLengthToSplitOn = 5;

	    public void CalculateDay(IResourceCalculationDataContainer resourceContainer, ISkillResourceCalculationPeriodDictionary skillStaffPeriods)
        {
	        var relevantProjections = resourceContainer as IResourceCalculationDataContainerWithSingleOperation;
			if (relevantProjections==null) return;

            foreach (var skillStaffPeriodDictionary in skillStaffPeriods.Items())
            {
                foreach (var pair in skillStaffPeriodDictionary.Value.Items())
                {
                    var period = pair.Key;
                    var demandedTraff = pair.Value.FStaff;
                    var periodDistribution =
                        new PeriodDistribution(pair.Value, skillStaffPeriodDictionary.Key.Activity, period, _intraIntervalLengthToSplitOn, demandedTraff);

                    pair.Value.ClearIntraIntervalDistribution();

					periodDistribution.ProcessLayers(relevantProjections, skillStaffPeriodDictionary.Key);
                }
            }
        }
    }
 }
