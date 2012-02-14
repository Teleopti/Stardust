using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for BlockFinderResult
    /// </summary>
    public interface IBlockFinderResult
    {
        /// <summary>
        /// Gets the shift category, could be null if no mandatory category was found.
        /// </summary>
        /// <value>The shift category.</value>
        IShiftCategory ShiftCategory { get; }

        /// <summary>
        /// Gets the block days, if the list is empty this is not a valid block.
        /// </summary>
        /// <value>The block days.</value>
        IList<DateOnly> BlockDays { get; }

        /// <summary>
        /// Gets the work shift finder result for the last call to NextBlock.
        /// </summary>
        /// <value>The work shift finder result. Returns null if valid block could be found</value>
        IDictionary<string, IWorkShiftFinderResult> WorkShiftFinderResult { get; }
    }
}