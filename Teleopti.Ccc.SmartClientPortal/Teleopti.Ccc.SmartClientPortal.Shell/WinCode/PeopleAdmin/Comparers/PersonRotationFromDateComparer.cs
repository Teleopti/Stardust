#region Imports

using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

#endregion

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
    /// <summary>
    /// Compares the From date of the person rotatios.
    /// </summary>
    /// <remarks>
    /// Created By: Savani Nirasha
    /// Created Date: 02-10-2008
    /// </remarks>
    //TODO:Temp fix. Need to consider this later
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    public class PersonRotationFromDateComparer<TAdapterParent, TBaseType, TScheduleType>
        : IComparer<TAdapterParent> where TAdapterParent : IRotationModel<TBaseType, TScheduleType>
    {
        #region IComparer<PersonRotationModelParent> Members

        /// <summary>
        /// Comparese the from date of two objects.
        /// </summary>
        /// <param name="x">A Person Rotation Grid Data object</param>
        /// <param name="y">A Person Rotation Grid Data object</param>
        /// <returns>Result of the comparisom</returns>
        public int Compare(TAdapterParent x, TAdapterParent y)
        {
            int result = 0;

            if (x.PersonRotation == null && y.PersonRotation == null)
            {
                // No need to set the value since the deault value equal to 0
            }
            else if (x.PersonRotation == null)
            {
                result = -1;
            }
            else if (y.PersonRotation == null)
            {
                result = 1;
            }
            else
            {
                // compares the CurrentRotation of the y with the teminal date of y
                result = DateTime.Compare(x.FromDate.Value.Date, y.FromDate.Value.Date);
            }

            return result;
        }

        #endregion
    }
}
