using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for PreferenceAbsenceChecker
    /// </summary>
    public interface IPreferenceAbsenceChecker
    {
        /// <summary>
        /// Check on scheduleday if preference is satisfied or not
        /// </summary>
        /// <param name="preference"></param>
        /// <param name="permissionState"></param>
        /// <returns></returns>
        PermissionState CheckPreferenceAbsence(IPreferenceRestriction preference, PermissionState permissionState);
    }
}
