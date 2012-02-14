using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Basic shift swap functionality
    /// </summary>
    public interface ISwapService
    {
        /// <summary>
        /// Inits the specified selected schedules.
        /// </summary>
        /// <param name="selectedSchedules">The selected schedules.</param>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-09-01    
        /// /// </remarks>
        void Init(IList<IScheduleDay> selectedSchedules);

        /// <summary>
        /// Determines whether it is legal to swap assignments between specified selected schedules.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance can swap assignments; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-09-04
        /// </remarks>
        bool CanSwapAssignments();

        ///// <summary>
        ///// Determines whether it is legal to swap days off between specified selected schedules.
        ///// </summary>
        ///// <returns>
        ///// 	<c>true</c> if this instance can swap days off; otherwise, <c>false</c>.
        ///// </returns>
        ///// <remarks>
        ///// Created by: micke
        ///// Created date: 2008-09-04
        ///// </remarks>
        /////bool CanSwapDaysOff();

        /// <summary>
        /// Swaps two assignments if permitted.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-09-04
        /// </remarks>
        IList<IScheduleDay> SwapAssignments(IScheduleDictionary schedules);

        ///// <summary>
        ///// Swaps the days off if permitted.
        ///// </summary>
        ///// <param name="schedules">The schedule ranges needed to do the swap.</param>
        ///// <param name="swapAssignmentsAsWell">if set to <c>true</c> swaps the assignments as well.</param>
        ///// <returns></returns>
        ///// <remarks>
        ///// Created by: micke
        ///// Created date: 2008-09-05
        ///// </remarks>
        /////IList<ISchedulePart> SwapDaysOff(IScheduleDictionary schedules, bool swapAssignmentsAsWell);
    }
}
