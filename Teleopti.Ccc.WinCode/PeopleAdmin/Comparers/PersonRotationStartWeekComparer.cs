#region Imports

using System.Collections.Generic;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

#endregion

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
    /// <summary>
    /// Compares the Start week of the person rotatios.
    /// </summary>
    /// <remarks>
    /// Created By: Savani Nirasha
    /// Created Date: 02-10-2008
    /// </remarks>
    //TODO:Temp fix. Need to consider this later
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
    public class PersonRotationStartWeekComparer<TAdapterParent, TBaseType, TScheduleType>
        : IComparer<TAdapterParent> where TAdapterParent : IRotationModel<TBaseType, TScheduleType>
    {
        #region IComparer<PersonRotationModelParent> Members

        /// <summary>
        /// Comparese the Start week of two objects.
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
                int objectValueX = x.StartWeek;
                int objectValuey = y.StartWeek;

                if (objectValueX < objectValuey)
                {
                    result = -1;
                }
                if (objectValueX > objectValuey)
                {
                    result = 1;
                }
            }

            return result;
        }

        #endregion
    }
}
