using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for BlockFinders
    /// </summary>
    public interface IBlockFinder
    {
        /// <summary>
        /// Returns the next list of dates that should be considered a block
        /// or an empty list if no block could be created.
        /// </summary>
        /// <returns></returns>
        IBlockFinderResult NextBlock();

        /// <summary>
        /// Gets the schedule matrix.
        /// </summary>
        /// <value>The schedule matrix.</value>
        IScheduleMatrixPro ScheduleMatrix { get; }

        /// <summary>
        /// Resets the block pointer.
        /// </summary>
        void ResetBlockPointer();
    }
}