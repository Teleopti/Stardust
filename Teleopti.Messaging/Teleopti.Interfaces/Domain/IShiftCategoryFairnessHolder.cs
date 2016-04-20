using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
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
		/// Gets the fairness value result.
		/// </summary>
		/// <value>The fairness value result.</value>
    	IFairnessValueResult FairnessValueResult { get; }

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