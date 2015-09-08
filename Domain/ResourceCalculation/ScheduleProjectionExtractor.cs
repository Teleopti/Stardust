using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
    /// <summary>
    /// Used to get all projections out of ScheduleDictionary, regardless of user rights
    /// </summary>
    public sealed class ScheduleProjectionExtractor : IScheduleExtractor
    {
	    private readonly int _minResolution;
	    private readonly ResourceCalculationDataContainer retList;

	    public ScheduleProjectionExtractor(IPersonSkillProvider personSkillProvider, int minResolution)
	    {
			retList = new ResourceCalculationDataContainer(personSkillProvider, minResolution);
		    _minResolution = minResolution;
	    }

	    /// <summary>
        /// Creates the relevant projection list.
        /// </summary>
        /// <param name="scheduleDictionary">The schedule dictionary.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-05-27
        /// </remarks>
        public ResourceCalculationDataContainer CreateRelevantProjectionList(IScheduleDictionary scheduleDictionary)
        {
		    using (PerformanceOutput.ForOperation("Creating projection"))
		    {
#pragma warning disable 618

				retList.Clear();

				scheduleDictionary.ExtractAllScheduleData(this);

				return retList;

#pragma warning restore 618
		    }
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
	        using (PerformanceOutput.ForOperation("Adding schedule part"))
	        {
		        retList.AddScheduleDayToContainer(schedulePart, _minResolution);
	        }
        }
    }
}
