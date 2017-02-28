using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Interface for shift category fairness
    /// </summary>
    public interface IShiftCategoryFairnessHolder
    {
        /// <summary>
        /// Gets the shift category fairness dictionary.
        /// </summary>
        /// <value>The shift category fairness dictionary.</value>
        IDictionary<IShiftCategory, int> ShiftCategoryFairnessDictionary { get; }


        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the other parameter; otherwise, false.
        /// </returns>
        bool Equals(IShiftCategoryFairnessHolder other);
    }
}