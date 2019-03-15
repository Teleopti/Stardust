using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Used to get all projections out of ScheduleDictionary, regardless of user rights
    /// </summary>
    public sealed class ScheduleProjectionExtractor : IScheduleExtractor
    {
	    private readonly ResourceCalculationDataContainer retList;

	    public ScheduleProjectionExtractor(IEnumerable<ExternalStaff> bpoResources, IPersonSkillProvider personSkillProvider, int minResolution, bool primarySkillMode)
	    {
			retList = new ResourceCalculationDataContainer(bpoResources, personSkillProvider, minResolution, primarySkillMode);
	   }

        /// <summary>
        /// Creates the relevant projection list.
        /// </summary>
        /// <param name="scheduleDictionary">The schedule dictionary.</param>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-27
        /// </remarks>
        public ResourceCalculationDataContainer CreateRelevantProjectionList(IScheduleDictionary scheduleDictionary, DateTimePeriod period)
        {
	        using (PerformanceOutput.ForOperation("Creating projection"))
	        {
		        retList.Clear();
				
				scheduleDictionary.ExtractAllScheduleData(this, period);

		        return retList;
	        }
        }

        void IScheduleExtractor.AddSchedulePart(IScheduleDay schedulePart)
        {
			retList.AddScheduleDayToContainer(schedulePart);
        }
    }
}
