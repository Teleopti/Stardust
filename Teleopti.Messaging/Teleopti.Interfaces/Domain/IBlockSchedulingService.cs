using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for BlockSchedulingServiceNew
    /// </summary>
    public interface IBlockSchedulingService
    {
        /// <summary>
        /// Occurs when [block scheduled].
        /// </summary>
        event EventHandler<BlockSchedulingServiceEventArgs> BlockScheduled;
		/// <summary>
		/// Executes the block scheduling process
		/// </summary>
		/// <param name="matrixList">The matrix list.</param>
		/// <param name="schedulingOptions">The scheduling options.</param>
		/// <param name="workShiftFinderResultList">The work shift finder result list.</param>
		/// <returns></returns>
        bool Execute(IList<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions,
            IDictionary<string, IWorkShiftFinderResult> workShiftFinderResultList);

    }
}