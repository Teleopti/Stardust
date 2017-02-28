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
    /// Compares the Current rotaion of the person rotatios.
    /// </summary>
    /// <remarks>
    /// Created By: Savani Nirasha
    /// Created Date: 02-10-2008
    /// </remarks>
    //TODO:Temp fix. Need to consider this later
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    public class PersonRotationCurrentRotationComparer<TAdapterParent, TBaseType, TScheduleType>
        : IComparer<TAdapterParent> where TAdapterParent : IRotationModel<TBaseType, TScheduleType>
    {
        #region IComparer<TAdapterParent> Members

        /// <summary>
        /// Comparese the Current rotation two objects.
        /// </summary>
        /// <param name="x">A Person Rotation Grid Data object</param>
        /// <param name="y">A Person Rotation Grid Data object</param>
        /// <returns>Result of the comparisom</returns>
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
                    string.Compare(((IRotation)x.CurrentRotation).Name, ((IRotation)y.CurrentRotation).Name,
                                   StringComparison.CurrentCulture);
            }

            return result;
        }

        #endregion
    }
}
