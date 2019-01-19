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
                foreach (var pair in skillStaffPeriodDictionary.Value.OnlyValues())
                {
                    var period = pair.CalculationPeriod;
                    var demandedTraff = pair.FStaff;
                    var periodDistribution =
                        new PeriodDistribution(pair, skillStaffPeriodDictionary.Key.Activity, period, _intraIntervalLengthToSplitOn, demandedTraff);

                    pair.ClearIntraIntervalDistribution();

					periodDistribution.ProcessLayers(relevantProjections, skillStaffPeriodDictionary.Key);
                }
            }
        }
    }
 }
