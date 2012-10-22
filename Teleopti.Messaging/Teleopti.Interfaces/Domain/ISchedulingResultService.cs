using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Service providing scheduling result
    /// </summary>
    public interface ISchedulingResultService
    {
        /// <summary>
        /// Create or recreate scheduling result for the complete loaded period.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-08-22
        /// </remarks>
        ISkillSkillStaffPeriodExtendedDictionary SchedulingResult();

		/// <summary>
		/// Create or recreate scheduling result for the given period.
		/// </summary>
		/// <param name="periodToRecalculate">The period to recalculate.</param>
		/// <param name="toRemove">To remove.</param>
		/// <param name="toAdd">To add.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-08-22
		/// </remarks>
        ISkillSkillStaffPeriodExtendedDictionary SchedulingResult(DateTimePeriod periodToRecalculate, IList<IVisualLayerCollection> toRemove, IList<IVisualLayerCollection> toAdd);

    }
}