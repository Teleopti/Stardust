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
        bool Execute(IList<IScheduleMatrixPro> matrixList, BlockFinderType blockFinderType,
            IDictionary<string, IWorkShiftFinderResult> workShiftFinderResultList);

        ///<summary>
        ///</summary>
        ///<param name="matrixList"></param>
        ///<returns></returns>
        bool Execute(IList<IScheduleMatrixPro> matrixList);

    }
}