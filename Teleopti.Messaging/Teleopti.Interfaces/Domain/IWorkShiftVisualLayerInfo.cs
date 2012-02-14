using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Visaual layer collection for workshift
    /// </summary>
    public interface IWorkShiftVisualLayerInfo
    {
        /// <summary>
        /// The source work shift.
        /// </summary>
        /// <value>The shift category.</value>
        IWorkShift WorkShift { get;}

        /// <summary>
        /// Gets the visual layer collection.
        /// </summary>
        /// <value>The visual layer collection.</value>
        IVisualLayerCollection VisualLayerCollection { get; }

    }
}