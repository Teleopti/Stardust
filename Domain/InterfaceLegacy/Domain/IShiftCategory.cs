using System.Drawing;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Represents a shift category, a grouping of shifts
    /// </summary>
    public interface IShiftCategory : IAggregateRoot,
                                        IChangeInfo,
                                        IFilterOnBusinessUnit
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-26
        /// </remarks>
        Description Description { get; set; }

        /// <summary>
        /// Gets the color of a ShiftCategory
        /// </summary>
        Color DisplayColor { get; set; }

		int? Rank { get; set; }
    }
}
