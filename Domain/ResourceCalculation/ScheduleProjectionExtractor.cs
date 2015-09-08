using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
		private readonly ICollection<Task> _extractionTasks = new Collection<Task>();

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
        public async Task<ResourceCalculationDataContainer> CreateRelevantProjectionList(IScheduleDictionary scheduleDictionary)
        {
		    using (PerformanceOutput.ForOperation("Creating projection"))
		    {
#pragma warning disable 618

				retList.Clear();
				_extractionTasks.Clear();
				scheduleDictionary.ExtractAllScheduleData(this);

				await Task.WhenAll(_extractionTasks.ToArray());
				_extractionTasks.Clear();

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
        public async Task<ResourceCalculationDataContainer> CreateRelevantProjectionList(IScheduleDictionary scheduleDictionary, DateTimePeriod period)
        {
	        using (PerformanceOutput.ForOperation("Creating projection"))
	        {
		        retList.Clear();
				_extractionTasks.Clear();
		        scheduleDictionary.ExtractAllScheduleData(this, period);

		        await Task.WhenAll(_extractionTasks.ToArray());
				_extractionTasks.Clear();

		        return retList;
	        }
        }

        void IScheduleExtractor.AddSchedulePart(IScheduleDay schedulePart)
        {
	        using (PerformanceOutput.ForOperation("Adding schedule part"))
	        {
		        var task = retList.AddScheduleDayToContainer(schedulePart, _minResolution);
                _extractionTasks.Add(task);
	        }
        }
    }
}
