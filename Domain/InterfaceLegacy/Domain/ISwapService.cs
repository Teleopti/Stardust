using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
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
        /// Swaps two assignments if permitted.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2008-09-04
        /// </remarks>
        IList<IScheduleDay> SwapAssignments(IScheduleDictionary schedules, bool ignoreAssignmentPermission, ITimeZoneGuard timeZoneGuard);
    }
}
