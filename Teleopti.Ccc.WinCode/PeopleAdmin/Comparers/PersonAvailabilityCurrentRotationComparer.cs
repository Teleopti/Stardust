#region Imports

using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;
using Teleopti.Interfaces.Domain;

#endregion

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
    /// <summary>
    /// Represents the comparer fot the person availability Current rotation.
    /// </summary>
    /// <remarks>
    /// Created by: Savani Nirasha
    /// Created date: 2008-10-15
    /// </remarks>
    //TODO:Temp fix. Need to consider this later
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    public class PersonAvailabilityCurrentRotationComparer<TAdapterParent, TBaseType, TScheduleType> 
        : IComparer<TAdapterParent> where TAdapterParent : IRotationModel<TBaseType, TScheduleType>
    {

        #region IComparer<TAdapterParent> Members

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>
        /// Value
        /// Condition
        /// Less than zero
        /// <paramref name="x"/> is less than <paramref name="y"/>.
        /// Zero
        /// <paramref name="x"/> equals <paramref name="y"/>.
        /// Greater than zero
        /// <paramref name="x"/> is greater than <paramref name="y"/>.
        /// </returns>
        /// <remarks>
        /// Created by: Savani Nirasha
        /// Created date: 2008-10-16
        /// </remarks>
        public int Compare(TAdapterParent x, TAdapterParent y)
        {

            int result = 0;

            if (x.CurrentRotation == null && y.CurrentRotation == null)
            {
                // No need to set the value since the deault value equal to 0
            }
            else if (x.CurrentRotation == null)
            {
                result = -1;
            }
            else if (y.CurrentRotation == null)
            {
                result = 1;
            }
            else
            {
                // compares the CurrentRotation of the y with the teminal date of y
                result =
                    string.Compare(((IAvailabilityRotation)x.CurrentRotation).Name, ((IAvailabilityRotation)y.CurrentRotation).Name,
                                   StringComparison.CurrentCulture);
            }

            return result;
        }

        #endregion

    }
}
