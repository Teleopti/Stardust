using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Shift category limitation
    /// </summary>
    public interface IShiftCategoryLimitation : ICloneable
    {
        /// <summary>
        /// Gets or sets the shift category.
        /// </summary>
        /// <value>The shift category.</value>
        IShiftCategory ShiftCategory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IShiftCategoryLimitation"/> is weekly.
        /// </summary>
        /// <value><c>true</c> if weekly; otherwise, <c>false</c>.</value>
        bool Weekly { get; set; }

        /// <summary>
        /// Gets or sets the max number of this shift category.
        /// </summary>
        /// <value>The max number of.</value>
        int MaxNumberOf { get; set; }
    }
}